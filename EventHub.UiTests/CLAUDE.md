# EventHub.UiTests

- Playwright + NUnit. Use page objects in `Pages/`; tests in `Tests/` contain no raw selectors.
- Prefer role/label/test-id locators over CSS/XPath; rely on Playwright auto-waiting, no fixed sleeps.
- Hybrid tests: set up and tear down data through the Core API client, then drive the UI.
