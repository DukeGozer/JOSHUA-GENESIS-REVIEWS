# JOSHUA GENESIS – öffentliche Expertenprüfung

## Neueste Expertenprüfung: GENESIS 0.3.1-R1

- [**Vollständiges R1-Quellcodepaket (25 Dateien, eine TXT)**](REVIEW_BUNDLE_0.3.1_R1.txt)
- [**Neuer Prüfauftrag für Gemini und Meta**](REVIEW_0.3.1_R1.md)

Exakter Quellcommit `b36c3555619f2b0df4edba7d703211886004dc25`; Windows-CI **43771075211: success** (UI, Host, Protokoll, Ollama-Vertrag, Paketlayout und End-to-End-Integration). Neuer Draft-PR [#7](https://github.com/DukeGozer/JOSHUA-SPIKE-5-GENESIS/pull/7), noch nicht gemergt. R1 ergänzt atomare Chatpersistenz, wiederholbare SQLite-Migration, ehrlichen Abbruchstatus, Wiederherstellung nach Host-Absturz, Quarantäne bei SQLite-Korruption, begrenzten vollständigen Gesprächskontext und UI-Stabilisierung.

**Noch keine reale Ollama/Qwen-GPU- oder interaktive WPF-/Mehrmonitor-Abnahme.** Nicht produktionsfreigegeben.

## Vorheriger Reviewstand: GENESIS 0.3.1 Alpha-Kandidat

- [**Quellcodepaket (25 Dateien)**](REVIEW_BUNDLE_0.3.1_ALPHA.txt)
- [**Prüfauftrag**](REVIEW_0.3.1_ALPHA.md)

## Vorheriger Reviewstand 0.3.0-R1

- [**Vollständiger Quellcode (22 Dateien, ein TXT)**](REVIEW_BUNDLE_0.3.0_R1.txt)
- [**Prüfauftrag nach Eigenprüfung**](REVIEW_0.3.0_R1.md)

R1 enthält gezielte Ollama-HTTP-Größenbegrenzungen, SQLite-Schema-Validierung und neue Regressionstests. Der exakte Windows-CI-Lauf **43733693930** ist erfolgreich. **Noch keine Produktionsfreigabe.**

## Vorheriger Entwicklungsstand: GENESIS 0.3.0

- [**Gesamter Quellcode als eine TXT-Datei (22 Dateien)**](REVIEW_BUNDLE_0.3.0.txt)
- [**Prüfauftrag für Gemini und Meta**](REVIEW_0.3.0.md)

Der Quellcode umfasst das neue SQLite-Fundament (0.2.0), das WPF Command Deck und den ersten lokalen Ollama-Chat (0.3.0). Der exakte Quellcommit hat einen **erfolgreichen automatischen Windows-Build** durchlaufen; Ollama wurde dabei nur über einen simulierten HTTP-Server geprüft. **Keine produktionsreife Freigabe.**

## Frühere Reviewstände

- [GENESIS 0.1.1 – Quellcode](REVIEW_BUNDLE_0.1.1.txt) · [Prüfauftrag](REVIEW_0.1.1.md)
- [GENESIS 0.1.0 – Quellcode](REVIEW_BUNDLE_0.1.0.txt) · [Prüfauftrag](REVIEW_0.1.0.md)

Das private Entwicklungsrepository bleibt privat. Dieses öffentliche Repository enthält nur bewusst bereitgestellte Quellcode-Kopien, keine alten JOSHUA NEXT/R57-Bestände oder privaten Nutzerdaten.
