# PROJECT JOSHUA SPIKE 5.0 GENESIS — unabhängige Prüfung 0.3.2-R1

## Exakter Prüfstand

**Quellcode:** [REVIEW_BUNDLE_0.3.2_R1.txt](REVIEW_BUNDLE_0.3.2_R1.txt) (vollständiges Text-Bundle mit 26 Dateien, Pfaden und Git-Blob-SHAs)

**Quellcommit:** `f195e0cb43832e77d5fcda2a45a7d08c4627193a` aus dem privaten Repository `DukeGozer/JOSHUA-SPIKE-5-GENESIS`

**Entwicklungs-PR:** https://github.com/DukeGozer/JOSHUA-SPIKE-5-GENESIS/pull/9 (Draft, **nicht gemergt**)

**Windows CI:** https://github.com/DukeGozer/JOSHUA-SPIKE-5-GENESIS/actions/runs/43845776429 (bitte den tatsächlichen Abschlusszustand selbst nachsehen und nicht aus dieser Beschreibung ableiten)

**Abnahme-Dokument:** `docs/GENESIS_0.3.2_R1_ACCEPTANCE.md`, im Bundle enthalten.

## Auftrag an Gemini und Meta

Bitte den vollständigen Quellcode des **genannten Commit** kritisch und unabhängig lesen, nicht nur die Zusammenfassung. Nachfolgend sind die konkreten Korrekturen aufgeführt; bitte prüft, ob die Ursachen und nicht nur die Symptome behoben wurden.

1. **Stop / Shutdown:** `StopSignal.Cancel()` wird auch ausgelöst, wenn nach einer bestätigten Stop-Anforderung der Pipe-Response fehlschlägt. Das Warten auf Chat-Tasks ist auf 15 Sekunden begrenzt. Sind dabei noch Laufzeit-, Datenverlust-, Zombieprozess- oder Shutdown-Races möglich?
2. **Concurrent Writes:** Der bereits in 0.3.2 vorhandene `lock(StateGate)` in `HandleClient()` umfasst `Execute()` einschließlich `remember` und `Save()`. Ein zuvor berichteter P0 Lost-Update-Fall war für diesen Codepfad falsch. Nun sendet der Integrationstest zwei gleichzeitige `remember`-Requests. Bitte nach anderen echten Datenrennen suchen, statt diesen hypothetischen Fehler zu wiederholen.
3. **Leere Ollama-Antwort:** Kein leerer `assistant`-Turn soll den späteren Chat-Kontext vergiften. `done_reason=length` wird als Fehler behandelt, nicht als vollständiger Erfolg. Prüft Ausnahmen, Teilantworten und Restart-Verhalten.
4. **Große Gesprächsrunden:** Überlange neue Assistant-Antworten dürfen nicht alle älteren vollständigen Kontextrunden abschneiden. Der Kontext darf keine Fehler-/Abbruchmarker als echte Assistant-Antworten erneut an Ollama übergeben. Prüft Budget, Auswahl, Reihenfolge und mögliche unvollständige Paare.
5. **Rollen-/Prompt-Isolation:** Es wird höchstens ein System-Turn übergeben. Gespeicherte Nutzernotizen werden als unverifizierte Daten gekennzeichnet; beurteilt, ob dadurch das vorherige Prompt-Injection-Risiko wirklich ausreichend begrenzt ist. Es besteht keine vollständige Mehrbenutzer-/Worker-Autorisierung.
6. **Statusgröße:** Der Host schickt einen kompakten *Wire-Snapshot* statt jede Sekunde sämtliche historischen Prompts und Resultate. Die SQLite-Historie bleibt vollständig. Prüft, ob UI-Zustände nach der Kompaktierung sinnvoll bleiben und ob die 4-MiB-Grenze unter den erlaubten Datenmengen eingehalten wird.
7. **Ollama-Kompatibilität:** Die Requests verwenden explizit `think=false`, `num_ctx=8192`, `num_predict=2048`. Prüft Modellkompatibilität, Token-Budget, Grenzen und Fehlerbehandlung. Die Optionswerte wurden nicht am echten Qwen getestet.
8. **Version:** Fenstertitel, Build-Metadaten und Paketname unterscheiden 0.3.2-R1 eindeutig von 0.3.2.
9. **Neue Tests:** Zwei parallele `remember`-Befehle, leere Antwort, 4500-Zeichen-Antwort und Erhalt älterer Kontextrunden, `done_reason=length`, doppelte Systemrolle, explizite Ollama-Optionen, Stop mit sofort getrenntem Client und große SQLite-Historie mit begrenzter Statusgröße.

## Gewünschtes Gutachten

Bitte liefere:
- Urteil **JA / JA MIT AUFLAGEN / NEIN** für eine **kontrollierte lokale Alpha**;
- bestätigte Fehler mit Dateipfad, Codebezug, Ursache und reproduzierbarem Ablauf;
- Trennung zwischen P0/P1/P2/P3, belegten Risiken und lediglich gewünschten Erweiterungen;
- Bewertung der automatisierten Integrationstests (einschließlich fehlender realer Streaming-/UI-/GPU-/Lock-Tests);
- konkrete verpflichtende Maßnahmen **vor** dem ersten Alpha-Test und Maßnahmen für spätere Meilensteine;
- Einschätzung, ob die Grenzen für HIVE, DARMOK, Lernregeln und weitere Worker ausreichend deutlich sind.

## Was ausdrücklich noch nicht bewiesen ist

- Kein echter Ollama-/Qwen-Durchlauf auf RTX 4070 SUPER mit 12 GB VRAM.
- Keine WPF-Interaktion, keine Drei-Monitor-/4K-/Misch-DPI-Abnahme.
- Keine GPU-Freigabemessung bei Cancel, kein mehrstündiger Soak-Test.
- Kein echter verzögerter HTTP-Chunk-Stream; Fake-Ollama sendet seinen NDJSON-Body am Stück.
- Keine Mehrbenutzer- oder vertrauliche-Daten-Sicherheitsfreigabe; SQLite speichert lokale Chatdaten im Klartext.
- Keine allgemeine Tokenzählung, keine Garantie über das Verhalten des 8192-Kontextfensters bei jedem Modell.
- Draft-PR und CI-Erfolg sind keine Produktfreigabe.

**Ziel ist ein langfristig tragfähiges Fundament, kein endloser Prototypenzyklus.** Bitte bevorzugt reproduzierbare Defekte gegenüber rein hypothetischen Umbauwünschen.
