# EventHub.TestAutomation

.NET 10 test automation solution for EventHub (API + UI).

## Structure
- `src/EventHub.Core/` – shared config, API client (RestSharp), models, data builders
- `EventHub.ApiTests/` – pure API tests (NUnit)
- `EventHub.UiTests/` – Playwright UI + hybrid tests (NUnit)

## Conventions
- Shared code lives in Core; test projects only contain tests (and page objects for UI).
- Add packages to the project that uses them (`dotnet add <project> package <Name>`).
- Never commit secrets; use `.env` / environment variables for credentials.

## Commands
- Build: `dotnet build`
- Run all tests: `dotnet test`
