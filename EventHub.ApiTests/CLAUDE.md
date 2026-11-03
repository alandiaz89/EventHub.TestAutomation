# EventHub.ApiTests

- Pure API tests; use the RestSharp client from `EventHub.Core`, no browser.
- Build request payloads with data builders from Core, not inline JSON.
- One test class per resource/endpoint group; assert status code, then body.
