---
description: Format page, component and elements classes (and their fixtures) to the project's page object conventions, without changing behavior.
argument-hint: [path-or-class-name ...]  (default: all of EventHub.UiTests/Pages and EventHub.UiTests/Elements)
allowed-tools: Read, Edit, Glob, Grep, Bash(dotnet build:*), Bash(git diff:*), Bash(git status:*)
---

Format the page object code to match how the project's page objects are implemented. The conventions come from the `new-page-object` and `new-locator-object` skills (read both first) and from the existing files, which are the reference for style.

**Scope:** $ARGUMENTS. If empty, use every `.cs` file in `EventHub.UiTests/Pages/` (including `Components/`) and `EventHub.UiTests/Elements/` (including `Components/`), plus `PageFixture.cs` and `ElementFixture.cs`. Touch nothing else (no tests, hooks or Core).

## What to normalize

**Page and component classes** (`Pages/`, `Pages/Components/`)
- File-scoped namespace (`namespace EventHub.UiTests.Pages;`), not a block namespace; no redundant braces around the namespace.
- `public sealed class <Name>Page(<Name>Elements elements)`, primary constructor, parameter named `elements`.
- Method names without an `Async` suffix. Single interactions use `Click<Element>`; multi-step flows use intent names (`Login`, `CreateEvent`).
- Methods return `Task`, not the next page. One-line bodies become expression-bodied (`=> await elements.X.ClickAsync();`); multi-step bodies keep braces.
- No assertions (`Expect`, `Assert`) and no `GetBy...` calls or raw selectors in page classes. If you find any, do not fix them silently: report them.

**Elements classes** (`Elements/`, `Elements/Components/`)
- File-scoped namespace, `public sealed class <Name>Elements(IPage page)` (or an `ILocator` root for a scoped component), `using Microsoft.Playwright;`.
- Expression-bodied `ILocator` properties; dynamic locators are methods returning `ILocator`.
- Group related locators with a short `// Section` comment; descriptive names (`SignInButton`, not `BtnPrimary`). No logic, actions, assertions or waits.
- Do not change a locator's strategy or text. That needs the live page and belongs to `new-locator-object`.

**Fixtures**
- `ElementFixture` and `PageFixture`: file-scoped namespace, `public sealed`, one auto-property per class in the form `public XElements XElements { get; } = new(page);` / `public XPage XPage { get; } = new(elements.XElements);`, consistent indentation, no stray blank or misaligned lines.
- Do not add a property for a class that does not exist (for example an elements class with no page class). Report it as unregistered or unused.

**All files:** `using` directives sorted, unused ones removed, one blank line between members groups, trailing whitespace and extra blank lines removed, file ends with a single newline, 4-space indentation, matching the style of the rest of the repo.

## Rules

- **Behavior-preserving only.** Formatting and structure to convention. Do not add, remove or rename public members, change locators, or change what a method does, except an `Async` suffix removal or a primary-constructor parameter rename needed by the conventions. When you rename a member, update every caller (`Grep` for usages in `Pages/`, `Tests/` and the fixtures) in the same run.
- **Report, don't redesign.** Conflicts, missing pieces or drift that cannot be fixed by formatting (assertions in a page, missing page class for an elements class, an unused constructor parameter, a skill that disagrees with existing code) go in the summary as findings with `file:line`. Do not decide them silently.
- Never touch secrets, config or generated folders (`bin/`, `obj/`, `allure-report/`).

## Steps

1. Read the two skills, then list the files in scope with `Glob` and read each one.
2. Make the edits file by file with `Edit`, smallest change that satisfies the conventions. Skip files that already conform.
3. Run `dotnet build EventHub.UiTests` and make sure there are no new errors or warnings caused by your changes.
4. Summarize: files changed (with a one-line reason each), files already conforming, renames made with the callers updated, findings you did not fix, and the build result. Say plainly what you did not run (this command does not run tests).
