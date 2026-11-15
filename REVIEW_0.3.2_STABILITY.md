# PROJECT JOSHUA SPIKE 5.0 GENESIS
## Unabhängige Expertenprüfung – Version 0.3.2 Stability Candidate

Wir bitten um eine neue, vollständige und kritische Gegenprüfung von GENESIS 0.3.2. Ziel ist ein möglichst belastbares Fundament für JOSHUA – nicht eine schnelle Freigabe um jeden Preis.

## 1. Exakter Prüfstand

- **Vollständiger Quellcode:** [REVIEW_BUNDLE_0.3.2_STABILITY.txt](REVIEW_BUNDLE_0.3.2_STABILITY.txt)
- **Exakter Quellcommit:** `78c29129906f03ef6d92dedf0b4c27e66937e106`
- **Privater Entwicklungs-PR:** https://github.com/DukeGozer/JOSHUA-SPIKE-5-GENESIS/pull/8 (Draft, nicht gemergt)
- **Windows-CI:** https://github.com/DukeGozer/JOSHUA-SPIKE-5-GENESIS/actions/runs/43775026013 — **SUCCESS**
- **Windows-Artefakt:** `GENESIS-0.3.2-win-x64-UNREVIEWED`, Artefakt-ID `14186485763`, verfügbar bis 22.11.2026.
- **Abnahmespezifikation:** enthalten im Bundle unter `docs/GENESIS_0.3.2_STABILITY_ACCEPTANCE.md`.

Das Bundle enthält 25 Dateien mit Originalpfaden und Git-Blob-SHAs. Bitte den Quellcode vollständig lesen und den angegebenen Commit als Prüfgrundlage verwenden. Nicht nur README, PR-Beschreibung oder diese Zusammenfassung prüfen. Falls eine Datei nicht zugänglich ist, bitte offen benennen.

## 2. Was gegenüber 0.3.1-R1 geändert wurde

### A. Persistenz-Fallback und Fail-Closed
- Wenn der finale Chat-Commit scheitert, versucht GENESIS eine gezielte Fallback-Schreiboperation.
- Scheitert auch diese, wird der In-Memory-Job sichtbar auf `failed` gesetzt, mit einem ausdrücklichen Persistenzfehler.
- Der Host sperrt danach schreibende Aktionen und erlaubt nur Statusabfrage und kontrolliertes Beenden.
- Scheitert beim Hoststart die Wiederherstellung eines unterbrochenen Chats einschließlich Fallback, bricht der Start ab, statt einen inkonsistenten Zustand zu präsentieren.

Bitte insbesondere prüfen, ob alle Speicherfehlerpfade, Zustandsübergänge und Worker-Lebenszyklen damit konsistent sind. Gibt es einen Weg, trotz Persistenzfehler erneut Schreiboperationen auszuführen oder einen Job fälschlich als aktiv zu zeigen?

### B. Auftragsverwaltung ohne Datenverlust
- Das Limit zählt aktive Jobs, nicht die gesamte Job-Historie.
- Laufzeit und UI laden die 500 jüngsten abgeschlossenen Jobs plus sämtliche aktiven Jobs.
- Ältere abgeschlossene Jobs bleiben vollständig in SQLite erhalten. Es gibt keine automatische Löschung.
- Ein Index unterstützt die Abfrage nach Status und Erstellungszeit.
- Ein Legacy-Import mit 505 abgeschlossenen Jobs ist Teil des Integrationstests; danach muss ein neuer Chat angenommen werden können.

Bitte prüfen, ob der SQL-Leseweg, die Laufzeitbegrenzung, das Sortieren und das Verhalten bei vielen aktiven Jobs korrekt sind. Gibt es versteckte Duplikate, verlorene Einträge oder eine andere dauerhafte Blockade?

### C. Shutdown-Aufnahmesperre
- Ein autorisierter Stop setzt die Aufnahmesperre vor dem Stop-Response.
- Bereits verbundene Clients dürfen danach keine neuen Aufträge starten.
- Der Host wartet kontrolliert auf aktive Clients, Worker und Chat-Tasks.

Bitte prüfen, ob es Race Conditions zwischen Stop, Chat-Annahme, Pipe-Akzeptanz und Worker-Start gibt.

### D. UI-Zustandsrennen
- Das Command Deck übernimmt die Job-ID direkt aus der Chat-Annahmeantwort.
- Ein In-Flight-Guard verhindert Doppelsenden.
- Ein Generation-Zähler verwirft ältere Status-Polls, wenn inzwischen eine neuere Chat-Annahme oder UI-Zustandsänderung erfolgt ist.
- Eine vorübergehende Nichterreichbarkeit verwirft eine bekannte aktive Job-ID nicht voreilig.

Bitte prüfen, ob ein verspäteter Timer-Poll, Host-Neustart oder ein sehr schneller Chatabschluss dennoch einen falschen Button- oder Cancel-Zustand erzeugen kann. Der WPF-Code wurde im CI kompiliert, aber nicht interaktiv auf dem Ziel-PC bedient.

### E. Fortschrittsupdates während des Streamings
- SQLite-Fehler eines nicht-terminalen Fortschrittsupdates sollen die Modellgenerierung nicht abbrechen.
- Der finale Commit bleibt maßgeblich.
- Ein Test-only Fault-Injection-Pfad erzeugt deterministisch einen einmaligen Fehler des Fortschrittsupdates während des Streams; der Stream muss trotzdem erfolgreich enden.

**Transparenz zur Testgrenze:** Dieser Regressionstest ist eine deterministische Fehler-Injektion, kein echter Mehrprozess-SQLite-Writer-Lock-Benchmark. Bitte bewertet, ob der produktive `SqliteException`-Pfad korrekt und ausreichend robust abgefangen wird.

### F. Vollständiger Modellkontext
- Beschlüsse werden vollständig übernommen oder ausgelassen, nicht mitten im Text abgeschnitten.
- Neueste Beschlüsse und jüngste vollständige Gesprächspaare haben Vorrang.
- Auslassungen werden im Modellkontext und Statushinweis ausdrücklich benannt.
- Gesprächshistorie enthält ausschließlich vollständige User-/Assistant-Paare.

Bitte untersuchen, ob die Auswahl wirklich die wichtigsten Beschlüsse erhält, die Reihenfolge stabil ist und Zeichenbudgets keine gefährlichen Kontextverluste verursachen.

## 3. Neue Regressionstests und CI-Nachweis

Der erfolgreiche Windows-Lauf umfasst:

1. Build von WPF-UI und Host unter Windows.
2. Protokoll-Roundtrip.
3. Ollama-API-Vertragstest.
4. Windows-Paketierung.
5. End-to-End-Hostintegration mit Fake-Ollama.
6. Job-Fenster: 505 abgeschlossene Jobs plus aktiver Job; ältere Datensätze bleiben in SQLite.
7. Legacy-JSON-Import mit mehr als 500 abgeschlossenen Jobs; ein neuer Chat bleibt möglich.
8. Beschlusskontext: vollständige neueste Beschlüsse plus expliziter Hinweis auf ausgelassene ältere Beschlüsse.
9. Stop-Aufnahmesperre: nach Stop keine neuen Chat-Aufträge.
10. Doppelte terminale Persistenzfehler: sichtbarer Failed-Zustand, Schreibsperre und Wiederherstellung beim normalen Neustart.
11. Einmaliger nicht-terminaler Fortschrittsfehler: Generierung läuft weiter und finale Antwort wird gespeichert.
12. Bestehende Migration-, Abbruch-, Neustart-, Quarantäne-, Pipe-Nebenläufigkeits- und Ollama-Fehlertests.

Die CI belegt den Build und die automatisierten Host-/SQLite-/Fake-Ollama-Tests. Sie belegt keinen realen lokalen Qwen-Lauf und keine interaktive WPF-Abnahme.

## 4. Bitte gezielt nach weiteren strukturellen Schwächen suchen

Nicht nur die genannten Punkte abhaken. Bitte auch angrenzende Bereiche untersuchen:

- SQLite-Transaktionen, WAL-Verhalten, Fehlercodes, Busy-Handling und Wiederherstellung
- Prozess-Lifecycle, Stop/Abbruch und Ressourcenfreigabe
- Pipe-Protokoll, ACLs, Control-Token und lokale Angriffsfläche
- Chat-Kontext, Session-Modell und Datenaufbewahrung
- UI-Threading, Timer, WPF-Rendering und Mehrmonitor-/DPI-Risiken
- Ollama-Streaming bei abgebrochener Verbindung, fehlendem `done=true`, langsamen oder leeren Chunks
- Speicherwachstum, Logs, SQLite-Dateigröße und Dauerbetrieb
- Architekturgrenzen zwischen Contracts, Host, Storage, UI und Ollama

Trennt dabei **bestätigte Fehler**, **begründete Risiken**, **nicht getestete Annahmen** und **bloße Verbesserungsvorschläge**.

## 5. Bekannte Grenzen – ausdrücklich nicht als getestet werten

- Kein echter Qwen-/Ollama-Lauf auf der RTX 4070 SUPER.
- Kein VRAM-Benchmark.
- Keine interaktive WPF-Abnahme auf dem Windows-11-Zielrechner.
- Keine Prüfung der zwei PC-Monitore plus 4K-TV und Misch-DPI.
- Kein mehrstündiger Dauerlauf.
- Kein echter Mehrprozess-SQLite-Lock-Benchmark; der Progress-Failure-Test verwendet eine Test-only Injektion.
- Keine Produktionsfreigabe.

## 6. Gewünschtes Gutachten

Bitte liefert:

1. **Freigabeurteil:** Ist 0.3.2 für eine kontrollierte lokale Alpha geeignet – ja/nein/mit Auflagen?
2. Bestätigte Korrekturen gegenüber R1.
3. Neu entdeckte Fehler, priorisiert P0/P1/P2/P3, jeweils mit Dateipfad und reproduzierbarem Szenario.
4. Begründete Risiken und nicht getestete Annahmen getrennt von bestätigten Fehlern.
5. Bewertung, ob die automatisierten Tests ihre Aussagen wirklich belegen.
6. Zwingende Korrekturen vor lokaler Alpha.
7. Verbesserungen, die ohne Sicherheits- oder Datenintegritätsrisiko bis M1 warten können.
8. Abschließende Einschätzung, ob GENESIS eine tragfähige Basis für JOSHUA mit persistentem Gedächtnis, Projekten, Aufgaben, HIVE, DARMOK und späteren Lern-Workern ist.

**Wichtig:** Bitte nicht aus Höflichkeit freigeben. Wenn ihr einen Fehler findet, beschreibt ihn konkret. Wenn ihr etwas nicht selbst ausgeführt habt, behauptet keinen Build oder Laufzeittest. Wir möchten ein starkes Fundament, keine künstliche Erfolgsmeldung.
