# GENESIS 0.3.0 — Gemeinsamer Prüfauftrag für Gemini und Meta

**Status:** experimenteller Reviewkandidat, keine produktionsreife Anwendung.

**Vollständige Codebasis**: [REVIEW_BUNDLE_0.3.0.txt](REVIEW_BUNDLE_0.3.0.txt) — 22 Dateien samt SHA-Werten und Originalpfaden. Enthält den 0.2.0-SQLite-Stand, das neue Command Deck und die 0.3.0-Ollama-Anbindung.

**Exakter privater Quellcommit:** `2621c8343461cc0fb173736d7fe59ff34495a3f4`  
**Windows-CI-Lauf:** `43712743494`, Ergebnis `success`. Host- und UI-Build, Protokoll-Smoke-Test, simulierte Ollama-API-Antworten, Paketstruktur sowie Windows-Host-Integration bestanden. Der CI-Runner besitzt **nicht** das Modell des Nutzers; Ollama wurde nur durch einen Mock geprüft.

## Gegenprüfung

Bitte das **gesamte** Bundle direkt lesen und die folgenden Punkte getrennt bewerten:

1. **SQLite/Persistenz 0.2:** Prüfung der transaktionalen Snapshot-Commits, Fehlerfälle, SQLite-Schema, WAL-Verhalten, Datenintegrität, Wiederaufnahme von Jobs und einmaliger Übernahme alter JSON-Snapshots. Besonders auf Migration nach Teilabsturz, gleichzeitig laufende Zugriffe und Datenverlust achten.
2. **WPF Command Deck:** XAML-Struktur, alle Click-Handler, Theme-Wechsel, Layoutbegrenzungen, dynamische Ressourcen, Hoststart, Verbindungsanzeigen und eventuelle UI-Blockaden. Deutlich zwischen Kompilierung und tatsächlichem UI-Test unterscheiden.
3. **Ollama 0.3:** Modellliste `GET /api/tags`, `POST /api/chat`, HTTP-Fehlerfälle, Cancel/Timeout, Race Conditions, JSON-Antwortbegrenzungen, lokaler Transport, Command-Lifecycle und eventuelle Modell-/Text-Injection-Risiken.
4. **Sicherheit:** Named-Pipe-Zugriff, `stop`-Capability aus einer Datei, Test-Environment-Overrides, sichere Dateipfade, DoS, Logs und bewusste Einschränkungen einer lokalen Same-User-Anwendung.
5. **Testqualität:** Quellcode der Tests lesen. Deckt CI die behaupteten Eigenschaften wirklich ab? Insbesondere JSON-zu-SQLite-Migration, Wiederanlauf, stop, parallele Clients, Ollama-Mock sowie Fehlerbehandlung.
6. **Freigabefrage:** Ist 0.3.0 für einen **kontrollierten lokalen Alpha-Test** geeignet? Keine Produktionsempfehlung ableiten, nur weil CI grün ist.

## Gewünschtes Ergebnis

- Kurze Gesamtbewertung für 0.2.0 und 0.3.0 getrennt
- **Reproduzierbare P0/P1-Bugs** mit exakt benannten Dateien, Ursache und Fix
- **Risiken/fehlende Tests** separat von bestätigten Bugs
- Konkrete minimale Korrekturen, priorisiert nach Nutzerwirkung
- Festhalten, was **nicht** getestet wurde

## Offene Funktionsgrenzen (keine falschen Versprechen)

- Nur Einzelanfragen: noch kein persistenter Multi-Turn-Chat und kein Token-Streaming
- Keine direkte Prüfung am echten Ollama-/Qwen-Modell oder an den Monitoren des Nutzers
- Kein eigenständiger Updater, kein HIVE, DARMOK, OCR oder GPU-Ressourcenmanager
- Die Host-Tests verwenden echte lokale Prozesse; der Ollama-HTTP-Test hingegen einen simulierten Handler
- Der SQLite-SPIKE speichert aktuell einen kompletten Snapshot pro Transaktion; das ist kein skalierter Wissensspeicher

Die public Reviewablage ist **nur** eine Codekopie; sie gewährt keinen Zugriff auf das private Entwicklungsrepository.
