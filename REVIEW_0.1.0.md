# GENESIS 0.1.0 — Public Code Review Snapshot

**Status: UNTESTED SPIKE — DO NOT INSTALL OR RUN AS A VERIFIED RELEASE.**

This repository is the public **review-only** snapshot of PROJECT JOSHUA SPIKE 5.0 GENESIS. The private development repository remains private. No JOSHUA NEXT/R57 sources, runtime state, models, datasets, credentials, or personal files are intended to be included.

## Read this before reviewing

- The code has **not been confirmed to build or run on Windows**.
- **Known P0 bug:** `Protocol.Json` uses `WriteIndented = true`, while Named Pipe messages are read with `ReadLineAsync()`. Multiline JSON breaks the message framing.
- The host's single-instance guard is an **exclusive file lock**, not a named OS mutex.
- Missing production safeguards include Pipe ACL restrictions, a versioned persistence schema, state recovery and an end-to-end run test.
- Additional review findings are welcome, especially compilation problems and runtime edge cases.

## Source layout

- `src/Genesis.Contracts/`: shared data types and wire protocol
- `src/Genesis.Host/`: background host process
- `src/Genesis.UI/`: desktop WPF client
- `scripts/build-release.ps1`: developer build script
- `.gitignore`: excludes generated output and local/private data

## Requested independent review

1. Inspect the entire source snapshot, **not only** `Protocol.cs`.
2. Identify compile-time blockers versus runtime blockers; quote file paths and concrete evidence.
3. Confirm or correct the known Pipe JSON framing finding.
4. Examine job persistence, threading, host lifecycle, input handling, and Windows WPF compilation.
5. Suggest a minimal complete `0.1.1` remediation plan with tests.
6. **Do not claim successful execution without a real build/test log.**

Reviewers can use the GitHub website or the raw file URLs. Public read access does not imply write access.

Snapshot provenance: **copied from the private GENESIS 0.1.0 source files on 2026-11-15**. This document is a review manifest, not an assertion of code correctness.
