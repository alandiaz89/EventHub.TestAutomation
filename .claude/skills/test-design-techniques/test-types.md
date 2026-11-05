# Test types

Classify each test condition by type so the team knows why it exists and where to run it. A condition can belong to more than one type.

## Functional testing
Verifies **what a system does**, ensuring its behavior meets the functional requirements.

| Type | Purpose | How to recognize / write the condition | EventHub example |
|---|---|---|---|
| **Smoke** | A shallow, fast check that the critical functions work at all, run first on every build. If it fails, stop and do not run the rest. | A few happy-path conditions across the main areas. | Login returns 200 and a token; `GET /events` returns 200. |
| **Sanity** | A narrow check, after a small change or fix, that the changed area works and nothing obvious broke. | Only the conditions around the change. | After fixing pagination, `page=1&limit=2` returns 2 events. |
| **Regression** | Re-run existing tests to confirm that a change did not break behavior that worked before. | The stable, automated suite; run on every change and in CI. | The full `GetEventsTests` set after touching `EventsInput`. |
| **Negative** | Verifies the system rejects or safely handles invalid input and unexpected use. | Invalid partitions and out-of-range boundaries; expected 4xx and a clear error. | `page=-1`, empty password, invalid token. |
| **API functional** | Verifies endpoint behavior directly through the API: status code, body, headers, side effects. | Request in, status code and body out, without a browser. Lives in `EventHub.ApiTests`. | `GET /events?category=Festival` returns only Festival events. |
| **UI functional** | Verifies behavior through the user interface: forms, navigation, messages. | Page object actions and visible outcomes. Lives in `EventHub.UiTests`. | The login form shows an error for a wrong password. |
| **E2E** | Verifies a complete user flow across layers (UI, API, database, third parties) from start to finish. | Scenario conditions that connect several features. Few, high-value, slow. | Register, log in, book an event, see it in My Bookings, cancel it. |

Guidance:
- Smoke and sanity are **selections** of conditions, not new conditions. Tag the existing ones (for example `[Category("Smoke")]`).
- Regression is the whole automated suite. New conditions added for a bug fix become regression tests.
- Negative conditions come from EP and BVA (invalid partitions, outside-the-boundary values).
- Prefer the lowest level that proves the condition: API functional before UI functional before E2E.

## Non-functional testing
Verifies **how well** the system does it, not only what it does. State a measurable target in the expected result.

| Type | Purpose | Example condition |
|---|---|---|
| **Performance** | Verify responsiveness under specified workloads. | Verify the API responds within 2 seconds with 1,000 users. |
| **Compatibility** | Verify the system can operate alongside other systems and exchange information correctly. | Verify the application exchanges data correctly with a third-party ERP system. |
| **Maintainability** | Evaluate how easily software can be analyzed, modified and tested. | Verify that a change to one module does not require extensive changes to unrelated modules. Check how easily the sort function can be changed from ascending to descending. |
| **Reliability** | Verify the system performs consistently without failure under specified conditions over time. | Run the application continuously for hours and verify it remains operational. |
| **Portability** | Verify how easily software can be transferred or adapted to different environments. | Verify an application can be deployed successfully on Windows or Linux. |
| **Security** | Verify the system protects information and prevents unauthorized access. | Verify users cannot access another user's account or data. |

Guidance for this project:
- **Performance:** the target site is a shared public practice site. Do not generate load against it. Limit work to a response-time sanity check (one request, a generous threshold) and flag real load testing as out of scope unless the user provides a safe environment.
- **Security:** cover missing, invalid and expired tokens, a normal user on admin endpoints, and access to another user's bookings or events. Use only accounts and data you created or own.
- **Compatibility and portability:** relevant for the UI suite (browser engines, viewport sizes) and for CI (running on Windows, macOS and Linux). Note that the Playwright config selects the browser.
- **Reliability:** repeated runs of the same test (see the `flaky-test-check` skill) are a practical reliability signal for the tests themselves.
- **Maintainability:** applies to the automation code too: page objects, builders and clients that make a change touch one place.
- Write non-functional conditions with an `N` prefix and a measurable expected result. If the target is unknown, mark it "unverified" and ask the user for the threshold.
