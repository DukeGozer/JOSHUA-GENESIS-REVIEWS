# GENESIS 0.3.0-R1 – Review nach Eigenprüfung

**Neueste vollständige Reviewfassung:** [REVIEW_BUNDLE_0.3.0_R1.txt](REVIEW_BUNDLE_0.3.0_R1.txt). Die bisherige [0.3.0-Datei](REVIEW_BUNDLE_0.3.0.txt) bleibt unverändert als Vergleich verfügbar.

**Privater Quellcommit:** `4cd839943e405c5cded7cc5b145dad77f7bc4792`.
**Windows CI:** [Lauf 43733693930](https://github.com/DukeGozer/JOSHUA-SPIKE-5-GENESIS/actions/runs/43733693930), **success**. Host, WPF, Protokolltest, Ollama-Mock-Vertragstest, SQLite-/Windows-Integration und Paketerstellung erfolgreich. Keine echte lokale Ollama- oder UI-Abnahme.

Die 22-Dateien-Basis entspricht dem bisherigen 0.3.0-Reviewstand mit exakt fünf aktualisierten Dateien:

- `src/Genesis.Ollama/OllamaClient.cs`: HTTP-Antwortgröße auf 2 MiB begrenzt, bevor JSON geparst wird; gilt für Modellliste und Chat.
- `src/Genesis.Storage/SnapshotStore.cs`: initialisierte SQLite-Datenbanken mit fehlender Schemaversion werden abgelehnt.
- `tests/Genesis.OllamaSmoke/Program.cs`: Regressionstests für ungültiges JSON und übergroße Antworten.
- `tests/Genesis.IntegrationSmoke/Genesis.IntegrationSmoke.csproj`: Storage-Referenz für gezielte SQLite-Prüfung.
- `tests/Genesis.IntegrationSmoke/Program.cs`: Entfernen der SQLite-Schemaversion testen; Testverbindungen ohne Pooling.

**Bitte prüfen:** Ist der HTTP-Byte-Limiter korrekt implementiert? Gibt es Grenzen, ab denen normale Ollama-Antworten unnötig gekappt werden? Sind SQLite-Metadaten, Migration, Zustandsschutz und ihre Tests ausreichend? Zusätzlich alle übrigen Punkte aus dem [ursprünglichen Prüfauftrag](REVIEW_0.3.0.md) betrachten.

Der Stand bleibt ein **Entwicklungs-/Alpha-Reviewkandidat**. Er enthält noch keinen Multi-Turn-Verlauf, keine Streaming-Ausgabe, keine manuelle UI-Abnahme und keinen realen Ollama-Leistungstest. Ein grüner CI-Lauf bedeutet nicht Produktionsfreigabe.
