# Claude Code GitHub Actions setup

This repository is wired for the direct Claude API workflow described in the Claude Code GitHub Actions docs.

## What was added

- `CLAUDE.md` at the repo root so Claude Code on GitHub sees the same repo guidance as `AGENTS.md`
- `.github/workflows/claude.yml` so `@claude` works in issue comments, PR review comments, PR reviews, and issue bodies/titles
- a local `.env` template for storing the secret value before you add it to GitHub

## What you need

1. Repository admin access for this GitHub repository
2. A Claude API key from Anthropic
3. Permission to install the Claude GitHub app on this repository

## GitHub setup steps

1. Install the Claude GitHub app for this repository:
   - https://github.com/apps/claude
2. In the repository settings, add a GitHub Actions secret named `ANTHROPIC_API_KEY`
3. Paste the value from your local `.env` file into that secret
4. Push this branch and merge it when you are ready

## Local `.env` usage

The local `.env` file is only a safe place for you to store the value on your machine. GitHub Actions will not read it directly. The workflow reads the repository secret named `ANTHROPIC_API_KEY`.

## Test commands

After the workflow is on the default branch and the secret is set, test with one of these:

- `@claude review this change and call out any risks`
- `@claude implement the fix described in this issue`
- `@claude propose the next small step for this repo`
