# AGENTS

## Project

`Claw Links` is a cross-platform launcher for opening Discord links in a managed Firefox-based browser lane that is built from upstream Mozilla source and distributed under project-owned branding.

## Current direction

- Prefer Firefox as the browser runtime.
- Keep the product name as `Claw Links`.
- Avoid bold security claims in naming and docs.
- Design for Windows, macOS, and Linux from the start.
- Treat stronger sandboxing as an optional layer on top of the managed browser lane.

## Immediate priorities

1. Build the launcher CLI.
2. Define the browser bundle layout per OS.
3. Bootstrap a deterministic managed profile.
4. Automate self-built browser packaging.

## Guardrails

- Do not use Mozilla trademarks as the shipped product identity.
- Keep the managed browser/profile separate from the user's everyday browser state.
- Prefer small, reviewable changes over broad speculative scaffolding.
- Document licensing, redistribution, and packaging implications when they affect implementation choices.

## Repo expectations

- Put architecture and policy decisions under `docs/`.
- Keep checked-in config under `config/`.
- Ignore local runtime state, browser bundles, and generated build outputs unless explicitly needed for releases.

## Development Workflow

When starting any new feature, functionality, or meaningful change:

1. Create an appropriate feature branch before making changes.
2. Use a branch name with the `codex/` prefix when appropriate, for example `codex/add-spectator-delay`.
3. Reference a `todo.md` during the work.
4. Stay on the branch that matches the current task and do not mix unrelated changes across branches.
5. Before switching branches, make sure work is committed or stashed and the working tree is clean.
6. When working on multiple branches at once, keep one task per branch and use separate worktrees when practical.
7. Ensure the thread is associated with the active working branch before committing. If the thread was previously on `main` and work is now on `codex/...`, switch or confirm the thread to that feature branch. Never commit to `main` unless explicitly instructed.
8. Keep branches small and single-purpose so review, testing, and rollback are easier.
9. Rebase or merge from `main` regularly to keep long-lived branches current.
10. Do the implementation work on that branch.
11. Run the relevant tests and checks before committing.
12. Commit and push only after the work has been tested.
13. If the change is valid, merge it.

Goal: keep `main` stable and use feature branches as the default workflow.