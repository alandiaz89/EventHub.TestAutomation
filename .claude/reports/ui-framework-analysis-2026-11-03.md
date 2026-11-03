# UI Framework Analysis

Date: 2026-11-03
Scope: `src/EventHub.Core`, `EventHub.UiTests` (ApiTests only checked for structure)
Method: read-only audit following the `ui-framework-analyzer` skill. Compared against the 2026-11-02 report (`ui-framework-analysis-2026-11-02.md`).

## 1. Executive assessment

This is still an early-stage Playwright/NUnit framework with a sound shape: `ElementFixture` -> `PageFixture` -> `PlaywrightHooks`, sealed classes, no assertions inside page actions, no static state, and a fresh browser context per test. Since the previous audit the package versions were pinned, `RegisterPage` is now registered, the `Async`/`Open*` names were removed and the stray "Task" test name was fixed. The most important remaining weaknesses are the unguarded setup/teardown lifecycle, credentials hardcoded in tests, and a new test that creates an account on a shared live site on every run. `dotnet build` succeeds with 1 warning; no tests were executed in this audit, so confidence is high for structure and moderate for runtime behavior.

## 2. Architecture map

```
appsettings.json (src/EventHub.Core, copied to output) + appsettings.{TEST_ENV}.json + env vars
        |
TestConfig.Load() -> BrowserConfig
        |
PlaywrightHooks [SetUp] (EventHub.UiTests/Hooks/PlaywrightHooks.cs)
   Playwright.CreateAsync -> BrowserFactory.LaunchAsync -> CreateContextAsync -> NewPageAsync -> GotoAsync(BaseUrl)
   Elements = new ElementFixture(Page)   // Login/Register/Home/Header/Footer Elements (locators only)
   Pages    = new PageFixture(Elements)  // LoginPage, RegisterPage, HomePage, HeaderComponent, FooterComponent
        |
AuthTests : PlaywrightHooks  (5 tests; actions via Pages.*, assertions via Expect(Elements.*))
        |
[TearDown] context.Close -> browser.Close -> playwright.Dispose

Dependency direction: UiTests -> Core (config only). Core `Api/`, `Builders/`, `Models/` hold no source files; ApiTests has no tests.
Parallelism: UiTests = ParallelScope.Fixtures, 2 workers (one fixture today -> effectively sequential).
```

## 3. Findings (highest severity first)

### Confirmed

| Severity | Status | File:line | Finding | Why it matters | Smallest practical fix |
|---|---|---|---|---|---|
| High | Confirmed | `EventHub.UiTests/Hooks/PlaywrightHooks.cs:22-31, 37-39` | `[SetUp]` assigns `null!` fields one by one. If launch, context, page creation or `GotoAsync` throws, `_playwright`/`_browser` are never closed, and `[TearDown]` then throws `NullReferenceException` on `_context` (line 37), hiding the original error. (Unchanged since the previous audit.) | Leaked driver/browser processes; misleading failure output when the site or browser is unavailable. | Null-conditional cleanup in `TearDown`: `if (_context != null) await _context.CloseAsync();` etc. |
| High | Confirmed | `EventHub.UiTests/Tests/AuthTests.cs:28,29,42,50,51` | A real-looking email and password are literals in source (values deliberately not repeated here). Still present. | Contradicts root `CLAUDE.md` ("Never commit secrets"). Anyone with the repo can use the account. | Read from env vars (`EVENTHUB_USER`, `EVENTHUB_PASSWORD`) via a small Core config class; rotate the password. |
| Medium | Confirmed | `EventHub.UiTests/Tests/AuthTests.cs:18-23` | `RegisterLinkOpensRegisterPage` submits a real registration (`Register("new@email.com", ...)`) with a fixed email and asserts nothing after it. The test name says it only checks navigation. | Against a shared live site the second run hits "already registered" (or pollutes data); the final step has no assertion, so it can never fail meaningfully. | Drop the `Register(...)` call from this test, or move it to a separate test with a unique email (e.g. `$"user{Guid.NewGuid():N}@example.com"`) and an assertion. |
| Medium | Confirmed | `EventHub.UiTests/Tests/AuthTests.cs:17,25,32,39,46` | `[Order(1..5)]` on independent tests (`Logout` logs in itself). | Suggests a dependency that does not exist and conflicts with the independence rule in `.claude/agents/ui-automation-designer.md`. Becomes misleading once fixtures run in parallel. | Remove `[Order]`. |
| Low | Confirmed | `EventHub.UiTests/Pages/HomePage.cs:5` | Empty class with an unread constructor parameter; build warning CS9113 (the only warning). | Harmless stub, noisy build. | Add the first action, or drop the parameter until then. |
| Low | Confirmed | `Pages/LoginPage.cs:3-5`, `RegisterPage.cs:3-5`, `HomePage.cs:3-5` vs `Pages/Components/*.cs`, `PageFixture.cs:4` | Pages use block-scoped namespaces and `loginElements`-style parameter names; components/`PageFixture` use file-scoped namespaces and `elements`. `HomePage.cs` and `LoginPage.cs` lack a trailing newline. | Cosmetic inconsistency. | Align with the file-scoped template. |
| Low | Confirmed | `Tests/AuthTests.cs:14-15` | Private properties are camelCase (`loginPage`, `header`); line 21 has trailing whitespace; line 49 has a stray blank line. | Readability only. | Rename / tidy. |
| Low | Confirmed | `Elements/Components/FooterElements.cs:3`, `HeaderElements.cs:3` | Files live in `Elements/Components/` but declare namespace `EventHub.UiTests.Elements` (not `...Elements.Components`). | Folder/namespace mismatch; harmless today because `ElementFixture` relies on it. | Leave, or change namespace and usings together. |

### Risks (require validation)

| Severity | Status | File:line | Finding | Why it matters | Smallest practical fix |
|---|---|---|---|---|---|
| Medium | Risk | `src/EventHub.Core/appsettings.json:6-7` | Committed config has `Headless: false` and `--start-maximized`. | CI without `Browser__Headless=true` would try to open a headed browser and likely fail. | Default `true` in JSON; override locally via `appsettings.{TEST_ENV}.json`. |
| Medium | Risk | `src/EventHub.Core/Config/TestConfig.cs:16-21`, `appsettings.json:3` | No validation; a missing config gives `BaseUrl = ""`. `ApiBaseUrl` is the placeholder `"test"`. | Failure surfaces later as an obscure `GotoAsync` error. | Throw in `Load()` if `BaseUrl` is not an absolute URI. |
| Medium | Risk | `Driver/BrowserFactory.cs:43` | `SetDefaultTimeout` does not affect `Expect(...)` (default 5s). | `TimeoutSeconds = 30` while assertions still time out at 5s; flaky on slow environments. | `Assertions.SetDefaultExpectTimeout(...)` from config in the hooks. |
| Medium | Risk | `Elements/LoginElements.cs:8-9,11`, `HeaderElements.cs:7` | `GetByLabel("Email")`, `GetByLabel("Password")`, link `"Register"` match by substring; `HeaderElements.Root` is any `navigation` landmark. | Extra matches cause strict-mode violations. Locator uniqueness was not checked against the live page. | Use `Exact = true` where needed after validating on the live page. |
| Low | Risk | `Hooks/PlaywrightHooks.cs:22,25` | A full Playwright driver and browser start per test. | Fine at 5 tests; the main cost at ~20+. | Later, move `IPlaywright`/`IBrowser` to `[OneTimeSetUp]` and keep context per test. |
| Low | Risk | `UiTests/AssemblyInfo.cs:3-4` | `ParallelScope.Fixtures` with one fixture gives no parallelism. | No hazard now; tests inside a fixture stay sequential. | Split fixtures as the suite grows. |

### Optional improvements (not defects at this stage)

| Severity | Status | File:line | Finding | Why it matters | Smallest practical fix |
|---|---|---|---|---|---|
| Low | Optional | `Hooks/PlaywrightHooks.cs:34-40` | No failure screenshot/trace/video. | Listed as target behavior in the designer doc; not built yet. | Start tracing on the context; save on failure via `TestContext.CurrentContext.Result`. |
| Low | Optional | `AuthTests.cs:28,36,50` | Every test logs in through the UI; no storage state or API setup. | Planned hybrid layer; Core `Api/`/`Builders/` are empty. | Reuse storage state once protected-page tests exist. |
| Low | Optional | `AuthTests.cs:21,29,36,43,51,53` | Tests reach through `Elements.*` for assertions. | A reasonable convention (assertions in tests, locators from Elements); just note that tests depend on both fixtures. | None needed. |
| Low | Optional | `Driver/BrowserFactory.cs:5` | Class is not sealed. | Consistency with the other classes only. | Add `sealed`. |
| Low | Optional | `TestConfig.cs:9` vs `BrowserConfig.cs:9` | `TimeoutSeconds` at both levels; only the browser one is used. | Ambiguity about which wins. | Remove the unused top-level property. |
| Low | Optional | `Elements/Components/FooterElements.cs:9` | `Footer => Root` duplicates a private member; `Register/Home/Footer` elements are unused by tests. | Cost is low; skill says add only what tests need. | No action; prune later if still unused. |
| Low | Optional | `.gitignore:7`, root `CLAUDE.md` | `.env` is gitignored and mentioned in docs but nothing loads it. | A developer creating `.env` would find it silently ignored. | Document "env vars only" or add a loader. |

### Resolved since 2026-11-02

- Packages pinned (`EventHub.UiTests.csproj:14`, `EventHub.Core.csproj:13`).
- `RegisterPage` registered (`PageFixture.cs:9`).
- `Async`/`Open*` method names removed (`FooterComponent.cs`, `HeaderComponent.cs`).
- `TaskLoginWithInvalidEmail` renamed (`AuthTests.cs:33`).

## 4. Design principles review

| Principle | Verdict | Evidence |
|---|---|---|
| Encapsulation | Pass | Pages hold their elements as constructor-captured private state; locators live only in `Elements/*`, and `Page` is `protected` in the hooks. Public `ILocator` properties on Elements classes are the intended design. |
| Abstraction | Pass | Pages expose actions (`Login`, `Register`, `Logout`), not Playwright calls. Elements/Pages split is a useful boundary, not a pure wrapper. |
| Inheritance | Pass | A single, valid NUnit inheritance: `AuthTests : PlaywrightHooks`. No page inheritance. |
| Polymorphism | Not applicable | No behavior varies yet; the engine switch in `BrowserFactory.cs:17-33` is the only variation and is adequately handled. |
| SRP | Concern | `PlaywrightHooks` owns config loading, factory creation, lifecycle, navigation and fixture wiring (`:22-31`); acceptable at this size, but it is the file that will grow first (tracing, expect timeout, auth state). |
| OCP | Pass | New pages are added by new classes plus one line in each fixture; the browser engine list is a small switch. |
| LSP | Pass | No overridden members or substitution hazards. |
| ISP | Not applicable | No interfaces exist and none are needed. |
| DIP | Concern (minor) | `PlaywrightHooks` constructs `BrowserFactory` and `TestConfig` directly (`:23-24`). Pages depend on Elements classes (concrete), not on `IPage`. Reasonable for UI tests; a design preference, not a violation. |
| POM | Pass | Tests state scenarios; page actions contain no assertions; no arbitrary sleeps (Playwright auto-wait and `Expect` used); elements are scoped through `Root` locators for header/footer. Gap: `RegisterLinkOpensRegisterPage` mixes navigation checking with a state-changing action. |

## 5. Focused recommendations

1. **Make setup/teardown failure-safe** (`PlaywrightHooks.cs`). Smallest change:
   ```csharp
   // before
   await _context.CloseAsync();
   await _browser.CloseAsync();
   _playwright.Dispose();
   // after
   if (_context != null) await _context.CloseAsync();
   if (_browser != null) await _browser.CloseAsync();
   _playwright?.Dispose();
   ```
   (Declare the fields as nullable instead of `null!`.)
2. **Move credentials out of source** and rotate the password: expose them through an environment-backed config class in Core and read them in `AuthTests`.
3. **Fix `RegisterLinkOpensRegisterPage`**: either end it at the heading assertion, or split registration into its own test with a unique email and an assertion on the outcome. Remove `[Order]` at the same time.

## 6. Verification and limitations

- **Inspected:** `EventHub.TestAutomation.sln` project layout; both `.csproj` files; `AssemblyInfo.cs` (UiTests and ApiTests); `Driver/BrowserFactory.cs`; `Hooks/PlaywrightHooks.cs`; all `Elements/*` and `Elements/Components/*`; all `Pages/*` and `Pages/Components/*`; `Tests/AuthTests.cs`; `Config/*.cs`; `appsettings.json`; `.gitignore`; previous report.
- **Run:** `dotnet build` -> Build succeeded, 0 errors, 1 warning (CS9113 at `Pages/HomePage.cs:5`).
- **Not run / not verifiable:** the test suite was not executed, so pass/fail status and the registration behavior on the live site are unverified. Locator uniqueness was not checked against the live page. No CI configuration exists (no `.github`), so CI readiness is assessed from config only. `EventHub.ApiTests` and Core `Api/`/`Builders/` contain no source to review.
