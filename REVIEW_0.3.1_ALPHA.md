# PROJECT JOSHUA GENESIS 0.3.1 — unabhängige Alpha-Gegenprüfung

**Status:** Alpha-Reviewkandidat, NICHT produktionsreif, noch nicht interaktiv auf dem Nutzer-PC abgenommen.

**Vollständiger Quellcode:** [REVIEW_BUNDLE_0.3.1_ALPHA.txt](REVIEW_BUNDLE_0.3.1_ALPHA.txt) — **25 Dateien** (24 Programm-/Build-/Testdateien und eine Alpha-Abnahmeanleitung), Originalpfade und Git-Blob-SHA enthalten.

**Exakter privater Quellcommit:** `0857242bcc40e6e8354c841b556fed59ef629288`  
**Windows-CI (success):** [Run 43764447303](https://github.com/DukeGozer/JOSHUA-SPIKE-5-GENESIS/actions/runs/43764447303)  
**Privater Entwicklungs-PR:** [#6](https://github.com/DukeGozer/JOSHUA-SPIKE-5-GENESIS/pull/6)  
**Testpaket:** `GENESIS-0.3.1-win-x64-UNREVIEWED` als GitHub-Actions-Artefakt im privaten CI-Lauf. Kein öffentlich bereitgestelltes Binary.

## Bereits umgesetzte Prüfaufträge aus Gemini/Meta

1. **Asynchrone Chat-Jobs:** `chat` liefert umgehend eine Job-ID über die Pipe, Modellgenerierung läuft unabhängig davon. Status-/Steuerbefehle müssen erreichbar bleiben.
2. **Exklusiver ModelGate:** Maximal eine lokale Ollama-Generierung gleichzeitig; zweite Chat-Anfrage wird kontrolliert abgewiesen. `chat-cancel` und regulärer `cancel` signalisieren Abbruch.
3. **Ollama Streaming:** `stream=true`, JSONL-Fragmente, Größenlimits, Fehlercode-Diagnose, fortlaufender Job-Status, finaler Reply.
4. **SQLite v1→v2:** Transaktionale Migration, `chat_messages` Append-only, persistenter Verlauf und gezielte UPSERTs für Beschlüsse/Jobs.
5. **Kontext:** Die letzten Nachrichten und gespeicherte Beschlüsse werden als begrenzter lokaler Chat-Kontext an Ollama weitergegeben.
6. **Datenintegrität:** Strenge Datums-/Wertevalidierung, Quarantäne-Kopien bei unzulässigen Daten, ohne automatische Löschung der Datenbank.
7. **Command Deck:** Light/Dark, Chat-Verlauf, Enter=Senden, Shift+Enter=Zeilenumbruch, Kopieren, Abbrechen, Fehleranzeige. Ein separater `Genesis.Launcher.exe` befindet sich im Stammordner des Windows-Pakets.
8. **Tests:** Mock-Ollama-API-Vertragstest sowie Windows-Host-Integration über Named Pipe und lokalem Fake-Ollama (GET models, POST chat stream, Chat-Abbruch, Status parallel), SQLite v1→v2 und Datenkorruption. Die CI hat die vollständigen Schritte erfolgreich durchlaufen.

## Bitte kritisch und unabhängig prüfen

- **P0/P1:** Race Conditions beim Chat-Start, beim Wechsel der CancellationTokens und beim Host-Stopp. Ist GPU-Semaphor in allen Fehlerpfaden garantiert freigegeben? Können abgebrochene Aufgaben nachträglich als erledigt erscheinen?
- **Persistenz:** Sind `Commit` (UPSERT) und `AppendChatMessage` bei Crashs konsistent? Läuft die Migration 1→2 wirklich transaktional und wiederholbar? Kann die Quarantäne bei WAL-Betrieb vollständige Dateien sichern?
- **Streaming und Nebenläufigkeit:** Bleiben `status`, `stop` und `cancel` auch unter mehrfachem langsamen Ollama-Streaming ansprechbar? Gibt es ungebremste Speicherbelegung oder blockierende 1-Zeichen-Reads?
- **Kontext und Nutzbarkeit:** Werden Nachrichten, Beschlüsse und Modellantworten richtig in der Modellhistorie repräsentiert und langfristig korrekt geladen? Unnötiger Verlust oder Duplizierung von Nachrichten? Performance der regelmäßigen WPF-Aktualisierung?
- **Sicherheit:** Umfang von `GENESIS_ALLOW_TEST_OVERRIDES`, lokale HTTP-Bindung, same-user Control-Token und schreibbare Benutzerverzeichnisse.
- **Tests:** Prüft die CI tatsächlich Host↔Pipe↔Fake-Ollama und Abbruch, oder fehlen entscheidende Fälle? Fehlende Tests ausdrücklich benennen.

## Gewünschter Bericht

1. Gesamturteil zur *kontrollierten lokalen Alpha-Freigabe* (JA / NEIN / nur eingeschränkt)
2. Bestätigte Fehler mit konkretem Quellpfad, Ursache, reproduzierbarem Szenario und Fix
3. Risiken und ungetestete Annahmen getrennt von Fehlern
4. Priorisierte Maßnahmen vor echter Windows-/Qwen-Abnahme
5. Keine Behauptung eines eigenen Builds, wenn ausschließlich Code gelesen wurde

## Explizite Grenzen

Der Windows-Lauf hat **keinen echten Qwen-/Ollama-GPU-Lauf auf der RTX 4070 SUPER** und **keinen interaktiven WPF-/DPI-/3-Monitor-Test** durchgeführt. Ein grüner CI-Lauf ersetzt keine manuelle Abnahme. Das SQLite-Schema wurde auf Version 2 angehoben; ein Rücksprung zu GENESIS 0.3.0-R1 ist ohne Wiederherstellen einer vorherigen Datensicherung nicht vorgesehen. Die Runtime und die UI sollten noch auf dem Anwender-PC getestet werden.
