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
- Update docs, configs, migrations, and changelog notes when the change affects them.
- Never commit secrets, local env files, or generated noise unless explicitly intended.

## Development Workflow

When starting any new feature, functionality, or meaningful change:

1. Fetch and sync with `main` before creating a new branch so work starts from a current base.
2. Create an appropriate feature branch before making changes.
3. Use a branch name with the `codex/` prefix when appropriate, for example `codex/add-spectator-delay`.
4. Reference a `todo.md` during the work.
5. Link the branch, PR, or commits to the relevant issue, task, or `todo.md` item for traceability.
6. Stay on the branch that matches the current task and do not mix unrelated changes across branches.
7. Before switching branches, make sure work is committed or stashed and the working tree is clean.
8. When working on multiple branches at once, keep one task per branch and use separate worktrees when practical.
9. Ensure the thread is associated with the active working branch before committing. If the thread was previously on `main` and work is now on `codex/...`, switch or confirm the thread to that feature branch. Never commit to `main` unless explicitly instructed.
10. Keep branches small and single-purpose so review, testing, and rollback are easier.
11. Rebase or merge from `main` regularly to keep long-lived branches current.
12. Open a draft PR early when helpful so the branch has visibility, discussion, and CI feedback.
13. Do the implementation work on that branch.
14. Keep commits focused and descriptive; avoid unrelated changes in the same commit.
15. Run the relevant tests and checks before committing.
16. Commit and push only after the work has been tested.
17. Resolve conflicts on the feature branch, not on `main`.
18. Re-run relevant tests after any rebase, merge, or conflict resolution.
19. Prefer `--force-with-lease` over `--force` when a rewritten branch must be pushed.
20. Avoid force-pushing shared branches unless necessary and understood.
21. If pausing work, leave the branch in a recoverable state with a clean commit or clearly named stash.
22. Include rollback notes for risky changes, especially schema, infra, packaging, or config changes.
23. Prefer one PR per branch and one branch per PR.
24. Require green CI and any needed review before merging.
25. If the change is valid and approved, merge it.
26. After merge, delete merged branches and remove unused worktrees.

Goal: keep `main` stable and use feature branches as the default workflow.

## Claude Review and Merge Gate

Before merging any feature branch to `main`:

1. Ensure there is an active PR for the branch. Open or update a draft PR early if needed.
2. Request Claude review on the PR by tagging `@claude` in a comment. Use an explicit review request when helpful, for example `@claude review`.
3. The coding agent must monitor Claude's PR comments, review feedback, and follow-up requests before merging.
4. If Claude identifies issues or requests changes, record the work in `todo.md`, implement the fixes on the same feature branch, rerun the relevant tests and checks, push the updates, and request Claude review again.
5. Continue this loop until Claude explicitly approves the changes or there is no unresolved blocking feedback from Claude.
6. Do not merge to `main` while Claude review is pending, while Claude has unresolved requested changes or blocking comments, or while CI is failing.
7. If Claude feedback conflicts with project requirements, prior approved decisions, or test results, do not loop indefinitely. Summarize the conflict in the PR and `todo.md`, then escalate for human resolution.
8. If multiple agents are involved, only one agent may be the active writer for a branch at a time. Other agents should review, comment, or work on separate branches or worktrees.
9. Resolve review feedback on the feature branch, not on `main`.
10. The coding agent is responsible for confirming the PR is in a mergeable state before merge; never assume a push alone is enough.

## Multi-Agent Coordination

- Use one agent as the primary implementer and another as reviewer or challenger unless explicitly directed otherwise.
- Do not allow multiple agents to commit directly to the same branch concurrently.
- If competing implementations are needed, give each agent its own branch or worktree from the same base and reconcile through review.
- Cap automated back-and-forth review and fix cycles to a small number of rounds before escalating.
- Keep `todo.md`, PR notes, and commit messages current so decisions, tradeoffs, and unresolved items remain traceable.