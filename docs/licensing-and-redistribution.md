# Licensing And Redistribution

This note is an engineering summary of Mozilla's published licensing and trademark rules. It is not legal advice.

## Short answer

Yes, we can compile Mozilla Firefox source ourselves and redistribute our modified build.

But:

- the code is under the MPL, so source-code obligations apply when we distribute executables
- modified builds cannot be redistributed using Mozilla trademarks such as `Firefox` without Mozilla's prior written permission

## MPL 2.0 implications

Mozilla publishes Firefox code under the Mozilla Public License 2.0.

The two most important practical points for this project are:

1. MPL copyleft is file-scoped rather than whole-program.
2. If we distribute executables, we must also make the MPL-covered source available and tell recipients how to obtain it.

Mozilla's own FAQ says MPL is not "viral" in the broad GPL sense. New files that contain no MPL-licensed code do not automatically have to become MPL simply because they ship alongside MPL files in a larger work.

Official references:

- MPL FAQ: <https://www.mozilla.org/en-US/MPL/2.0/FAQ/>
- MPL text, Section 3.2 summary: <https://www.mozilla.org/zh-CN/MPL/2.0/>

Useful Mozilla FAQ points:

- Q11: file-level copyleft, not whole-program copyleft
- Q10: if you distribute an executable based on modified MPL code, you must make the MPL-covered source available and inform recipients how to obtain it

## What we should plan to publish

For every distributed browser release, we should plan to publish:

- the exact upstream Mozilla revision we built from
- our patch set or fork source for all modified MPL-covered files
- a copy of the MPL 2.0
- a clear source-offer notice in release artifacts and on the download page

For the launcher code we write from scratch, we can choose our own license later. It does not have to be MPL unless we copy MPL-covered code into it.

## Trademark rules are separate from copyright license rules

This is the key constraint.

Mozilla's distribution policy says:

- unaltered Firefox can be redistributed under Mozilla trademarks only if you follow Mozilla's unaltered distribution rules
- if you modify Firefox, you may not redistribute that modified product with Mozilla trademarks without prior written consent

Mozilla gives explicit examples of changes that trigger the modified-build rule, including:

- adding or deleting source files
- changing installer files
- changing defaults
- adding extensions, add-ons, or plugins

Official references:

- Mozilla Trademark Guidelines: <https://www.mozilla.org/en-US/foundation/trademarks/policy/>
- Mozilla Distribution Policy: <https://www.mozilla.org/en-US/foundation/trademarks/distribution-policy/>

The most important line for us is Mozilla's statement that if you make changes, you may not distribute that product using Mozilla trademarks and may not continue to call it Firefox without prior written consent.

## What this means for our project

Because we want:

- a hardened browser configuration
- custom policy defaults
- our own launcher and packaging
- likely custom installer or embedded distribution

we should assume from the start that our shipped browser must:

- use a new product name
- use new icons and branding
- avoid Firefox logos and Mozilla marks in app identity
- clearly state that it is based on Mozilla Firefox source code

## Redistribution decision table

Allowed without special Mozilla trademark permission:

- compiling Firefox source for internal/private use
- modifying Firefox source for internal/private use
- redistributing modified builds under a non-Mozilla name while complying with MPL source obligations

Not safe to assume allowed without written permission:

- redistributing our modified browser as `Firefox`
- using Firefox logos or Mozilla branding for our modified app
- presenting the modified app as an official Mozilla build

## Operational takeaway

The hard part is not the open-source license. The hard part is:

- update cadence
- release engineering
- macOS signing/notarization
- avoiding Mozilla trademark misuse

So the safest path is:

1. self-build from upstream Mozilla source
2. ship under our own name and branding
3. publish source and notices for each release
4. keep security update turnaround tight
