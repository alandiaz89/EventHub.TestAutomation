# EventHub.TestAutomation

Automated tests for [EventHub](https://eventhub.rahulshettyacademy.com/), a practice event-booking site, written in C# on .NET 10. The solution covers the REST API, the web UI, and hybrid tests that set up data through the API and then drive the UI.

| Layer | Tools |
|---|---|
| API tests | NUnit, RestSharp |
| UI tests | NUnit, Playwright for .NET |
| Reporting | Allure (`Allure.NUnit`) |
| CI | GitHub Actions (`.github/workflows/tests.yml`) |

The tests run against a shared public site. Every test creates its own data and cleans it up, so tests can run in parallel and do not depend on each other.

## Solution layout

```
EventHub.TestAutomation.sln
├── src/EventHub.Core/        Shared code used by both test projects
│   ├── Config/               TestConfig, ApiConfig, BrowserConfig (appsettings.json + environment variables)
│   ├── Clients/              ApiClient (RestSharp transport) and one service class per API area:
│   │                         AuthClient, EventsClient, BookingsClient; ClientFixture exposes them
│   ├── Models/               Request and response models
│   ├── Builders/             Data builders that produce unique, valid test data
│   ├── TestData/             Value lists the builders pick from
│   └── Cleanup/              CleanupRegistry: runs cleanup actions even when a test fails
├── EventHub.ApiTests/        Pure API tests
│   ├── Hooks/ApiHooks.cs     Per-test client setup, login helpers, cleanup
│   ├── Assertions/ApiAssert  Shared assertion helpers
│   └── Tests/                Auth and Events tests (including an end-to-end CRUD lifecycle test)
└── EventHub.UiTests/         Playwright UI and hybrid tests
    ├── Driver/               BrowserFactory (engine, headless, timeouts from config)
    ├── Hooks/                PlaywrightHooks (browser lifecycle, trace and screenshot on failure)
    │                         HybridHooks (adds an API client and cleanup for hybrid tests)
    ├── Elements/             One class of locators per page or component; ElementFixture exposes them
    ├── Pages/                One class of user actions per page or component; PageFixture exposes them
    └── Tests/                Auth, event creation and booking tests
```

### Conventions

- Shared code (clients, models, builders, config) lives in `EventHub.Core`. Test projects contain tests and their own hooks. The UI project also holds page and elements classes.
- **UI:** the Page Object Model is split in two. Elements classes hold locators, page classes hold actions and no assertions, and tests assert through the elements. Locators prefer role, label and test id over CSS; there are no fixed sleeps.
- **API:** one service class per API area (`<Area>Client`) with no assertions. Tests assert on status code first, then body, and write the expected status as a number (`Is.EqualTo(404)`).
- Fixtures and page objects are created per test with plain constructor injection. There is no DI container and no static mutable state.
- Never commit secrets. Credentials come from environment variables or a local file that git ignores.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PowerShell (`pwsh`), used once to install the Playwright browsers
- Google Chrome, because `appsettings.json` sets `"Engine": "chrome"` (see Configuration to change it)

## Setup

```bash
dotnet build

# Install the Playwright browsers (path is relative to the repo root)
pwsh EventHub.UiTests/bin/Debug/net10.0/playwright.ps1 install --with-deps chromium
```

Some tests log in as an existing user. Provide its credentials as environment variables:

```bash
export UserEmail="you@example.com"
export UserPassword="your-password"
```

Alternatively, put them in `src/EventHub.Core/appsettings.local.json`, which git ignores:

```json
{
  "UserEmail": "you@example.com",
  "UserPassword": "your-password"
}
```

## Configuration

Settings are read, in this order, from: `appsettings.json`, `appsettings.{TEST_ENV}.json` (when the `TEST_ENV` variable is set), `appsettings.local.json`, then environment variables. Later sources override earlier ones.

| Setting | Meaning | Default in `appsettings.json` |
|---|---|---|
| `BaseUrl` | Web app URL | https://eventhub.rahulshettyacademy.com/ |
| `ApiBaseUrl` | API URL (must be an absolute URL) | https://api.eventhub.rahulshettyacademy.com/api |
| `UserEmail`, `UserPassword` | Existing user for tests that log in | not set |
| `Browser:Engine` | `chrome`, `msedge`, `chromium`, `firefox` or `webkit` | `chrome` |
| `Browser:Headless` | Run without a visible window | `false` |
| `Browser:SlowMo` | Milliseconds to wait between actions | `0` |
| `Browser:TimeoutSeconds` | Default timeout | `30` |

Environment variables use `__` for nesting, for example `Browser__Headless=true` or `Browser__Engine=chromium`.

`chrome` and `msedge` launch the installed Google Chrome or Microsoft Edge. The other engines use the browsers that `playwright.ps1 install` downloads.

## Running the tests

```bash
dotnet test                                   # everything
dotnet test EventHub.ApiTests                 # API tests only
dotnet test EventHub.UiTests                  # UI and hybrid tests only
dotnet test --filter "Category=E2E"           # one category
dotnet test --filter "FullyQualifiedName~DeleteEventTests"   # one class by name
Browser__Headless=true dotnet test EventHub.UiTests          # UI tests without a window
```

### Test categories

| Category | Meaning |
|---|---|
| `API`, `UI` | The project the test belongs to |
| `Auth`, `Events`, `Bookings` | The feature area |
| `E2E` | Multi-step flows: the API CRUD lifecycle and the hybrid UI tests |
| `Product-Bug` | The test asserts the behavior the API documents, but the live API does not do it yet. These tests are expected to fail until the product is fixed. |

To run everything except the known product bugs:

```bash
dotnet test --filter "Category!=Product-Bug"
```

### Parallel execution

Both projects run whole fixtures in parallel (`ParallelScope.Fixtures`): 4 workers for the API tests and 2 for the UI tests. Tests inside one fixture run one after another. The setting lives in each project's `AssemblyInfo.cs`, so it applies locally and in CI without any extra configuration.

## Reports and diagnostics

- **Allure:** results are written to an `allure-results` folder in each project's build output, for example `EventHub.ApiTests/bin/Debug/net10.0/allure-results`. Open them with the [Allure command-line tool](https://allurereport.org/docs/install/) (`allure serve <folder>`).
- **UI failures:** a screenshot, the page URL, browser console errors and a Playwright trace are attached to the report. Traces are also saved under `traces/` next to the test binaries.
- **API failures:** every request sent by the failing test (method, URL, status, bodies) is printed to the test output and attached to the Allure report. Tokens and authentication request bodies are never recorded.

## Continuous integration

`.github/workflows/tests.yml` runs on pushes to `main` and `AlanBranch`, and on pull requests. It has two jobs, `api-tests` and `ui-tests`, which run at the same time on separate runners. The API job runs the whole API project without a filter, so the `Product-Bug` tests fail the pipeline until the API is fixed.

The repository needs two secrets, `EVENTHUB_USER_EMAIL` and `EVENTHUB_USER_PASSWORD`. Pull requests from forks do not get secrets, so the tests that log in as the configured user fail there.

## Adding tests

- **New API test:** add the client method, models and builder to `EventHub.Core`, then the test class in `EventHub.ApiTests/Tests/`, inheriting `ApiHooks`. Register anything the test creates for cleanup (`TrackEvent`).
- **New UI test:** add or extend a page and its elements class, register them in `PageFixture` and `ElementFixture`, then write the test inheriting `PlaywrightHooks`.
- **New hybrid test:** inherit `HybridHooks`. It gives you `Clients` for API setup, and helpers such as `RegisterUser` and `DeleteEventAfterTest` for data the test creates.

The API contract, with its endpoints, schemas and known differences from the live API, is in `.claude/skills/references/api-contract.md`.

## Working with Claude Code

The `.claude` folder holds project-specific agents and skills used with Claude Code, for example agents for designing API and UI tests, reviewing code, debugging failures and auditing the framework architecture. See `CLAUDE.md` for the project instructions.
