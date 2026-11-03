# UI Framework Analysis

Date: 2026-11-02
Scope: `src/EventHub.Core` and `EventHub.UiTests`
Method: `ui-automation-designer` agent, following the `ui-framework-analyzer` skill (read-only audit).

## 1. Executive assessment

This is a small, early-stage Playwright/NUnit framework, and the structure is sound. The fixture composition (`ElementFixture` -> `PageFixture` -> `PlaywrightHooks`) matches the stated design, with sealed classes, no assertions in pages, no static state, and a fresh browser context per test. All 4 UI tests pass.

The weakest points are:
- `PlaywrightHooks` has no cleanup if setup fails.
- Credentials are hardcoded in the tests.
- NuGet packages float with `Version="*"`.
- Several files drift from the page-object skill: `Async`/`Open*` method names, a `RegisterPage` that is never registered, and navigation that doesn't return the next page.

Confidence is high for the UI project and config. It is lower for locator uniqueness, because the live page was not inspected.

## 2. Architecture map

```
appsettings.json (src/EventHub.Core, copied to output)  +  env vars  +  TEST_ENV
        |
TestConfig.Load()  -> BrowserConfig
        |
PlaywrightHooks [SetUp]  (EventHub.UiTests/Hooks/PlaywrightHooks.cs)
   Playwright.CreateAsync -> BrowserFactory.LaunchAsync -> CreateContextAsync -> NewPageAsync
   Elements = new ElementFixture(Page)    // LoginElements, RegisterElements, HomeElements, HeaderElements, FooterElements
   Pages    = new PageFixture(Elements)   // LoginPage, HomePage, Header, Footer   (RegisterPage missing)
   Page.GotoAsync(BaseUrl)
        |
AuthTests : PlaywrightHooks  -> act via Pages.*, assert via Elements.*
[TearDown] Context.Close -> Browser.Close -> Playwright.Dispose
```

`EventHub.Core` has the `Api/`, `Builders/` and `Models/` directories, but they contain no files. `EventHub.ApiTests` has no tests.

## 3. Findings (highest severity first)

### Confirmed

| Severity | Status | File:line | Finding | Why it matters | Smallest practical fix |
|---|---|---|---|---|---|
| High | Confirmed | `EventHub.UiTests/Hooks/PlaywrightHooks.cs:19-40` | `[SetUp]` assigns the `null!` fields one at a time. If launch, context creation or `GotoAsync` throws, `_playwright` and `_browser` are never closed. NUnit still runs `[TearDown]`, which then hits a `NullReferenceException` on `_context` (line 37) and hides the original error. | The orphaned driver and browser processes leak. The real failure message is lost, which makes diagnosis hard. | Null-guard each field in `TearDown` (`if (_context is not null) ...`) and make the fields nullable. |
| High | Confirmed | `EventHub.UiTests/Tests/AuthTests.cs:20,27,34,42` (and `:21,43`) | A real-looking email and password are in source as literals. (The values are deliberately not repeated in this report.) | This contradicts the root `CLAUDE.md` ("Never commit secrets") and the designer doc's rule that secrets come from environment variables (`.claude/agents/ui-automation-designer.md:16`). Anyone with repo access can use the account. | Move them to env vars (for example `EVENTHUB_USER` / `EVENTHUB_PASSWORD`) read via config, and rotate the password. Keep the invalid-credential inputs as local test constants. |
| Medium | Confirmed | `EventHub.UiTests/EventHub.UiTests.csproj:14`, `src/EventHub.Core/EventHub.Core.csproj:13` | `Microsoft.Playwright.NUnit` and `RestSharp` use `Version="*"`. Every other package is pinned. | Builds are not reproducible. A new Playwright release can silently change browser requirements or behavior between runs and machines. | Pin the versions currently resolved. |
| Medium | Confirmed | `EventHub.UiTests/Pages/PageFixture.cs:8-11` | `RegisterElements` is registered (`ElementFixture.cs:8`), but `RegisterPage` is not exposed. | The page class (`RegisterPage.cs`) is unreachable from tests. This breaks step 5 of `new-page-object`. | Add `public RegisterPage RegisterPage { get; } = new(elements.RegisterElements);`. |
| Medium | Confirmed | `EventHub.UiTests/Pages/Components/FooterComponent.cs:7,9,11`, `HeaderComponent.cs:13` | The methods are named `OpenBrowseEventsAsync`, `OpenMyBookingsAsync`, `OpenManageEventsAsync` and `ClickAdminMenuAsync`. | The skill and designer doc say page and component methods have no `Async` suffix, and single interactions use `Click<Element>`, not `Open`. The header's other methods (`HeaderComponent.cs:7-15`) follow the rule, so this is also inconsistent within the project. | Rename to `ClickBrowseEvents`, `ClickMyBookings`, `ClickManageEvents` and `ClickAdminMenu`. |
| Medium | Confirmed | `EventHub.UiTests/Pages/LoginPage.cs:7,14`, `RegisterPage.cs:7,15` | `Login`, `Register`, `ClickRegisterLink` and `ClickSignInLink` return `Task`. The docs say navigation returns the next page object. | The docs and code disagree. Returning a page after `Login` is awkward because login can fail, as in the negative tests. | Decide and document the rule. Either return the next page for unambiguous links like `ClickRegisterLink`, or note in the skill that flows with branching outcomes return `Task`. Do not change the code yet. |
| Medium | Confirmed | `EventHub.UiTests/Tests/AuthTests.cs:17,24,31,38` | `[Order(n)]` is used, although the tests are independent and `Logout` re-logs in. | `[Order]` suggests a dependency that doesn't exist. It also conflicts with the independence rule (`.claude/agents/ui-automation-designer.md:17`, `:24`). | Remove `[Order]`. |
| Low | Confirmed | `EventHub.UiTests/Pages/HomePage.cs:5` | The class is empty and the parameter is unread. The build prints warning CS9113. | Harmless scaffolding, but it is the only build warning. | Fine to keep as a planned stub. Add the first action or remove the parameter until then. |
| Low | Confirmed | `LoginPage.cs:3-5,18-19`, `RegisterPage.cs`, `HomePage.cs` vs `HeaderComponent.cs:3` | The pages use block-scoped namespaces and `loginElements`-style parameter names. The components and the skill template use file-scoped namespaces and `elements`. | Cosmetic inconsistency. | Align with the template. |
| Low | Confirmed | `AuthTests.cs:14-15,25` | Private properties are camelCase (`loginPage`, `header`). The test name `TaskLoginWithInvalidEmail` has a stray "Task". | Readability only. | Rename (for example `LoginWithInvalidEmail`). |
| Low | Confirmed | `Elements/*.cs`, `Pages/Components/*.cs` | `RegisterElements`, `HomeElements`, `FooterElements` and most `HeaderElements` members are unused by any test today. `Footer => Root` (`FooterElements.cs:9`) duplicates a private member. | The skill says "Keep it small. Add only what the tests need now." The cost is low, since the classes are small and stateless. | No action. Prune only if they go stale. |

### Risks (require validation)

| Severity | Status | File:line | Finding | Why it matters | Smallest practical fix |
|---|---|---|---|---|---|
| Medium | Risk | `src/EventHub.Core/appsettings.json:6-7` | The committed config has `Headless: false` and `--start-maximized`. | Without `Browser__Headless=true` in the environment, CI would try to open a headed browser. That needs a display, so it likely fails. | Default to `true` in the JSON and override locally via `appsettings.{TEST_ENV}.json` or env vars. |
| Medium | Risk | `src/EventHub.Core/Config/TestConfig.cs:16,21` | There is no validation. A missing config silently gives `BaseUrl = ""`. `ApiBaseUrl` is the placeholder `"test"` (`appsettings.json:3`). | The failure surfaces later as an obscure `GotoAsync` error. | Throw in `Load()` if `BaseUrl` is not an absolute URI. |
| Medium | Risk | `Driver/BrowserFactory.cs:43` | `SetDefaultTimeout` covers actions, but `Expect(...)` has its own default of 5s, which config does not control. | `TimeoutSeconds` can be raised to 30 while assertions still time out at 5s. This can cause flaky failures on slow environments. | Set `Assertions.SetDefaultExpectTimeout` from config in the hooks. |
| Medium | Risk | `Elements/LoginElements.cs:8-9,11`, `HeaderElements.cs:7` | `GetByLabel("Email")`, `GetByLabel("Password")` and the `"Register"` link all match by substring. `HeaderElements.Root` is `GetByRole(Navigation)`. | Any extra match (a "Show password" control, another nav landmark) causes a strict-mode violation. The tests pass today, but uniqueness was not checked on the live page. | Add `Exact = true` where it is safe, and confirm there is one nav landmark. |
| Low | Risk | `Hooks/PlaywrightHooks.cs:22,25` | A full Playwright driver and browser are started per test. | At 4 tests (17s total) this is fine. At about 20 tests it will be the main cost. | Defer. Later, move `IPlaywright` and `IBrowser` to `[OneTimeSetUp]` per fixture and keep the context per test. |
| Low | Risk | `UiTests/AssemblyInfo.cs:3-4` | `ParallelScope.Fixtures` with a single fixture gives no parallelism today. | There is no hazard now, but tests inside a fixture stay sequential. | Split fixtures as the suite grows (for example Auth, Register, Navigation). |

### Optional improvements (not defects at this stage)

| Severity | Status | File:line | Finding | Why it matters | Smallest practical fix |
|---|---|---|---|---|---|
| Low | Optional | `Hooks/PlaywrightHooks.cs:34-40` | There is no failure screenshot, trace or video. | The designer doc lists these as target behavior (`ui-automation-designer.md:27`). They are not built yet. | Add tracing to the context and save on failure in `TearDown`, using `TestContext.CurrentContext.Result`. |
| Low | Optional | `AuthTests.cs:20,34,42` | Every test logs in through the UI, and there is no storage state or API setup. | This is the planned hybrid layer. It is not a defect, because the Core `Api/` and `Builders/` directories are empty. | When protected-page tests arrive, create a storage state once and reuse it. |
| Low | Optional | `Driver/BrowserFactory.cs:5` | The class is not sealed. | The sealing rule only names page, elements and `PageFixture` classes, so there is no conflict. | Seal it for consistency. |
| Low | Optional | `TestConfig.cs:9` vs `BrowserConfig.cs:9` | `TimeoutSeconds` exists at both levels, and only the browser one is used. | It is ambiguous which one wins. | Remove the unused top-level property. |
| Low | Optional | `Elements/RegisterElements.cs:8-10` | These use test ids and a placeholder for fields that likely have labels. | The skill's locator priority puts label above test id and placeholder. This is acceptable if the page lacks accessible labels. | Re-check on the live page. |
| Low | Optional | `.gitignore:7` and root `CLAUDE.md` | `.env` is gitignored and the docs mention it, but nothing loads it. | A developer who creates `.env` would find it ignored. | Document "env vars only", or add a loader. |

## 4. Design principles review

| Principle | Verdict | Evidence |
|---|---|---|
| Encapsulation | Pass | Locators are exposed only through elements classes. Pages use their own elements (`LoginPage.cs`). Lifecycle fields are private (`PlaywrightHooks.cs:11-13`). |
| Abstraction | Pass | `BrowserFactory` isolates engine mapping. Pages expose `Login` and `Register` intent methods. There is no pointless driver wrapper. |
| Inheritance | Pass | The only inheritance is `AuthTests : PlaywrightHooks` (valid NUnit setup sharing). Pages, elements and fixtures are sealed and independent. |
| Polymorphism | Not applicable | Nothing varies behaviorally. The engine `switch` in `BrowserFactory.cs:17-33` is acceptable. |
| SRP | Pass | `ElementFixture` only builds elements, `PageFixture` only builds pages, and the hooks own only lifecycle. A minor concern is that `PlaywrightHooks` also navigates to the base URL (`:31`). |
| OCP | Pass | A new page needs one elements class, one page class and two fixture lines. The engine `switch` is a small, acceptable modification point. |
| LSP | Pass | There are no substitution hierarchies to break. |
| ISP | Not applicable | There are no interfaces, and none are needed. |
| DIP | Pass | Pages depend on injected elements, not on `IPage`. Tests depend on page abstractions, not selectors. |
| POM | Concern | The structure is good: no assertions in pages, shared Header/Footer components, and `Expect` only in tests. The concerns are the naming drift (`Async`/`Open*`), the unregistered `RegisterPage`, and navigation that doesn't return pages. |

## 5. Focused recommendations

1. **Make the hooks resilient and put credentials in config.** Make the fields nullable and guard each in `TearDown`. Read the credentials from env vars or config, and rotate the exposed password.
   ```csharp
   // before
   await _context.CloseAsync(); await _browser.CloseAsync(); _playwright.Dispose();
   // after
   if (_context is not null) await _context.CloseAsync();
   if (_browser is not null) await _browser.CloseAsync();
   _playwright?.Dispose();
   ```
2. **Pin the `*` package versions and set CI-safe defaults.** This means a pinned `Microsoft.Playwright.NUnit` and `RestSharp`, `Headless: true` in `appsettings.json`, and a `BaseUrl` validation in `TestConfig.Load()`. Wire the config timeout into Playwright's expect timeout as well.
3. **Bring the code and the docs back into line.**
   - Add `RegisterPage` to `PageFixture`.
   - Rename the `Async`/`Open*` component methods to `Click*`.
   - Remove `[Order]` from the tests.
   - Settle the "navigation returns the next page" rule for flows with branching outcomes like `Login`, and record it in the skill.

## 6. Verification and limitations

- **Inspected:**
  - Both agent and skill docs, plus the root and `EventHub.UiTests` `CLAUDE.md` files.
  - Every `.cs` file in `EventHub.UiTests` (Driver, Hooks, Elements, Pages, Tests, `AssemblyInfo.cs`).
  - `EventHub.UiTests.csproj`, `EventHub.Core.csproj`, `Config/*.cs`, `appsettings.json` and `.gitignore`.
- **Not inspected or not verifiable:**
  - The `.sln` file.
  - `.claude/settings.json` and `.mcp.json`.
  - `TestCases.md`.
  - The Core `Api/`, `Builders/` and `Models/` directories, which contain no files.
  - `EventHub.ApiTests` beyond its project reference and `AssemblyInfo.cs`.
  - Locator uniqueness and accessible labels on the live site.
  - Parallel behavior with more than one fixture.
  - CI behavior.
- **Ran:**
  - `dotnet build` succeeded with 0 errors and 1 warning (CS9113 at `HomePage.cs:5`).
  - `dotnet test --no-build` with the env var `Browser__Headless=true`, which does not modify any file. UiTests: 4 passed, 0 failed, 17s. ApiTests: no tests discovered, so there was nothing to run.
- **Side effect:** the test run logged in to the live site with the hardcoded account.
- **Files changed during the audit:** none.
