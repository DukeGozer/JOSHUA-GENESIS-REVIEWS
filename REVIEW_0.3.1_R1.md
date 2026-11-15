# PROJECT JOSHUA GENESIS 0.3.1-R1 — unabhängige Gegenprüfung

**Prüfstatus:** Vollständiger Review-Kandidat; Windows-CI erfolgreich. Weiterhin Draft/Alpha, nicht produktionsfreigegeben.

## 1. Exakter Prüfstand

- **Quellcommit:** `b36c3555619f2b0df4edba7d703211886004dc25`
- **Privater Entwicklungs-PR:** https://github.com/DukeGozer/JOSHUA-SPIKE-5-GENESIS/pull/7 (Draft; basiert auf dem 0.3.1 Alpha-Zweig)
- **Windows-CI:** https://github.com/DukeGozer/JOSHUA-SPIKE-5-GENESIS/actions/runs/43771075211 — **SUCCESS**
- **Vollständiges Quellcodepaket:** [REVIEW_BUNDLE_0.3.1_R1.txt](REVIEW_BUNDLE_0.3.1_R1.txt) — 25 Dateien, exakte Pfade und Blob-SHAs
- **Windows-Artefakt:** `GENESIS-0.3.1-R1-win-x64-UNREVIEWED`, nur im privaten Actions-Lauf, gültig bis 22.11.2026. Für dieses Code-Review bitte den Textbundle verwenden.

Der Windows-Lauf hat UI, Host, Protokolltest, Ollama-API-Vertragstest, Paketlayout und den vollständigen Host-Integrationstest erfolgreich ausgeführt.

## 2. Welche Befunde aus 0.3.1 wurden bearbeitet?

Die vorherigen Gutachten hatten insbesondere diese Schwachstellen benannt. Bitte kontrolliert unabhängig, ob die Ursachen tatsächlich beseitigt wurden:

1. **Orphaned Chat-Turns:** Chat-Job und User-Nachricht werden nun atomar in einer SQLite-Transaktion erstellt. Erfolgreiche, fehlgeschlagene und abgebrochene Chats erhalten jeweils eine Assistant-Abschlussnachricht. Beim Host-Neustart werden unterbrochene `queued`-, `running`- und `canceling`-Chats repariert und mit einem Assistant-Marker geschlossen.
2. **Snapshot-/SQLite-Schema:** `Snapshot.SchemaVersion` ist nun 2 und wird vor dem SQLite-Commit aus Legacy-JSON v1 auf 2 normalisiert. SQLite-Schema v1→v2 wird transaktional migriert.
3. **Idempotente Migration:** Vorhandene Spalten werden über `PRAGMA table_info` geprüft; fehlende Spalten werden nur ergänzt. Das Schema-Metadatum wird in derselben DDL-Transaktion gesetzt. Ein Neustart nach bereits ausgeführter DDL soll die Migration wiederholen können.
4. **Wahrheitsgemäßer Abbruchstatus:** Ein Chat zeigt zunächst `canceling`. Der Terminalstatus `canceled` wird erst nach Ende des Workers und Freigabe des ModelGate persistiert. Währenddessen wird eine zweite Generierung abgewiesen.
5. **SQLite-Fehlerquarantäne:** Neben ungültigen Anwendungsdaten werden auch `SqliteException`-Fehler abgefangen. Datenbank sowie vorhandene WAL-/SHM-Dateien werden in eine Quarantäne-Kopie geschrieben; das Original wird nicht still überschrieben.
6. **Write Amplification:** Streaming-Fortschritt aktualisiert gezielt den aktiven Jobdatensatz, statt alle Beschlüsse und Jobs alle 400 ms erneut zu serialisieren.
7. **Kontextbegrenzung:** Nur vollständige User-/Assistant-Paare werden als Gesprächshistorie verwendet. Budget: bis zu 5.000 Zeichen Beschlusskontext, 3.000 Zeichen jüngste abgeschlossene Historie, 8.000 Zeichen aktuelle Eingabe.
8. **Oberfläche:** Der letzte Chatfehler wird im Transcript angezeigt; Scrollposition und Textauswahl werden bei Aktualisierungen wiederhergestellt; das Chatpanel ist mit einem GridSplitter vertikal veränderbar.

## 3. Neue Regressionstests im Windows-CI

Der Integrationstest gegen einen lokalen Fake-Ollama-HTTP-Server prüft nun zusätzlich:

- Snapshot-/SQLite-Schema-Version 2 und SQLite-v1→v2-Migration mit erhaltenen Daten
- Wiederanlauf einer frischen, bereits teilweise angelegten Migration
- Atomare Chat-Erstellung: doppelter Message-Key darf keinen verwaisten Job hinterlassen
- Tatsächlich an Fake-Ollama gesendeten Kontext: gespeicherter Beschluss, aktuelle User-Eingabe und vorherige User-/Assistant-Runde
- Mid-Stream-Ollama-Fehler: Jobstatus, Fehlermeldung und Assistant-Fehlermarker
- Sichtbarer `canceling`-Status während verzögerter Freigabe, kein paralleler Modellstart und finaler Abbruchmarker
- Geordnetes Beenden des Hosts während einer laufenden Chatgenerierung
- Harter Host-Abbruch während Streaming und Reparatur des offenen Chats beim nächsten Start
- Quarantäne einer ungültigen SQLite-Datei sowie bereits vorhandener SQLite-Anwendungsdatenfehler
- Bereits bestehende Tests zu Pipe-Nebenläufigkeit, Größenlimits, Authentifizierung, Jobfortschritt, Restart und Persistenz

Der Test verwendet absichtlich einen test-only Verzögerungsparameter, der ausschließlich mit `GENESIS_ALLOW_TEST_OVERRIDES=1` aktiv ist, damit der Zwischenstatus `canceling` reproduzierbar geprüft werden kann.

## 4. Bitte besonders kritisch nachprüfen

Bitte nicht nur bestätigen, dass die beschriebenen Änderungen vorhanden sind. Prüft, ob sie unter realen Fehlerbedingungen wirklich korrekt sind:

- **Transaktions- und Speicherfehler:** Was passiert, wenn `FinishChat` oder der Fallback-`UpdateJobProgress` ebenfalls scheitert? Kann danach noch ein inkonsistenter Job oder unvollständiger Verlauf verbleiben?
- **ModelGate / Cancellation:** Gibt es irgendeinen Pfad mit doppeltem `Release`, fehlendem `Release`, verspätetem Cancel oder einem neuen Chat vor dem tatsächlichen Ende der alten Modellanforderung?
- **Host-Lifecycle:** Wird bei hartem Kill und bei Windows-Abmeldung konsistent wiederhergestellt? Welche Shutdown-Fälle sind noch ungetestet?
- **SQLite-Migration:** Ist die Migration bei allen möglichen Abbruchzeitpunkten idempotent? Ist die Kombination aus WAL, DDL-Transaktion und Quarantäne sicher?
- **Kontextbudget:** Ist die Zeichenbegrenzung als Näherung für Tokens belastbar? Werden wichtige Beschlüsse bei langen oder sehr vielen Entscheidungen ungewollt abgeschnitten? Ist die Reihenfolge der Gesprächsrollen immer gültig?
- **UI:** Ist die Wiederherstellung von Selection/Scroll in WPF tatsächlich stabil? Gibt es Dispatcher-/Timer-Races oder Probleme bei stark wachsenden Transcripts?
- **Security / Process:** Prüft die Test-Override-Abgrenzung ausreichend? Sind Pipe-ACL, Control-Token, lokaler Ollama-Endpunkt und Prozessende ausreichend geschützt?
- **Architektur:** Welche Teile sind noch SPIKE-Provisorien und sollten vor M1/M2 ersetzt werden?

## 5. Bekannte Grenzen – bitte nicht als getestet annehmen

- Kein echter Qwen-/Ollama-Durchlauf auf der RTX 4070 SUPER.
- Kein interaktiver WPF-Test auf dem Windows-PC.
- Keine echte Prüfung der drei Monitore, des 4K-TV oder der gemischten DPI-Skalierung.
- Kein mehrstündiger Dauerlauf oder VRAM-Wechselbenchmark.
- Der UI-Scroll-/Selection-Code und der Resizer wurden kompiliert, aber nicht per Maus auf dem Zielgerät abgenommen.
- Die 8.000-Zeichen-Grenze der aktuellen Eingabe ist eine bewusst konservative Alpha-Grenze; bitte auf Bedienbarkeit und Tokenrealismus prüfen.

## 6. Gewünschtes Ergebnis

Bitte liefert ein unabhängiges Gutachten mit:

1. **Freigabeurteil:** kontrollierte lokale Alpha ja / nein / nur mit Auflagen.
2. Bestätigte Fehler mit Dateipfad, Ursache und reproduzierbarem Szenario.
3. Risiken und nicht getestete Annahmen getrennt von bestätigten Fehlern.
4. Priorisierte Fixes, falls erforderlich.
5. Eine klare Aussage, ob die Tests die behaupteten Eigenschaften tatsächlich beweisen.
6. Keine Behauptung eines eigenen Builds oder eines realen GPU-Tests, sofern ihr diese nicht selbst ausgeführt habt.

Bitte berücksichtigt auch die vorherigen Gutachten und prüft, ob R1 die damals beanstandeten Ursachen behebt oder nur deren Symptome kaschiert.

**Ziel:** Nach einer weiteren unabhängigen Prüfung wollen wir GENESIS 0.3.1-R1 kontrolliert auf dem Windows-Rechner ausprobieren und anschließend zügig den nächsten nutzbaren JOSHUA-Meilenstein angehen.
