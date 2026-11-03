---
name: flaky-test-check
description: Prove whether an EventHub test is flaky by running it repeatedly (alone and in parallel with its fixture), comparing passing and failing runs, and reporting a pass rate with likely causes. Use when a test fails intermittently, passes locally but fails in CI, or before and after a flakiness fix. Then use diagnose-failing-test on the failing runs.
argument-hint: <TestName> [runs=10]
---

# Flaky test check

"Flaky" must be shown, not assumed: the same code both passes and fails. This skill produces that proof and a pass rate.

## Steps

1. **Build once:** `dotnet build --configuration Release`. Use `--no-build` for every run so the code is identical across runs.
2. **Run in isolation, N times** (default 10, ask before going above 20; the tests hit a shared public site):
   ```
   for i in $(seq 1 N); do dotnet test --no-build --configuration Release --filter "FullyQualifiedName~<TestName>" ; done
   ```
   Record pass/fail and the failure message for each run. Keep trace and Allure output of the failing runs (the hooks write traces on failure only; they are overwritten per run name plus a random id, so they accumulate).
3. **Run with its fixture in parallel:** run the whole test class (`--filter "FullyQualifiedName~<ClassName>"`) a few times. Tests run in parallel at fixture level, so this exposes shared data and ordering problems that the isolated run hides.
4. **Compare conditions** when results differ: headless vs headed (`Browser__Headless`), local vs CI (`.github/workflows/tests.yml` runs headless in Release), same account vs different, time of day (the shared site may be slow).
5. **Compare failing runs with passing ones.** Same failure message each time suggests a deterministic cause that only triggers sometimes (data, ordering). Different messages suggest timing or environment.
6. **Shortlist likely causes** from the evidence: missing web-first assertion or a wait on the wrong condition, locator matching more than one element sometimes, non-unique or shared test data, state left by another test, session expiry, slow responses from the site. Hand the failing runs to `diagnose-failing-test` for the root cause.
7. **After a fix, repeat steps 2 and 3 with the same N** and compare the pass rate. A single green run proves nothing.

## Report format

```
**Test:** <name>
**Isolated:** X/N passed (headless/headed)
**With class in parallel:** X/M passed
**Failure messages:** list with counts
**Differences found:** conditions that correlate with failure
**Verdict:** flaky | consistently failing | not reproducible in N runs
**Likely cause:** short, with the evidence
**Next step:** run diagnose-failing-test on <failing run> | apply fix and re-run
```

## Rules

- Do not add retries (`[Retry]`) to make a flaky test pass; they hide the defect.
- Do not edit code in this skill; it measures and reports. Fixes belong to the diagnosis step and need the user's go-ahead.
- "Not reproducible in N runs" is a valid result; say how many runs and under which conditions, not "fixed" or "not flaky".
- Never print or commit secrets; do not create or delete site data beyond what the test already does.
- State what you ran and what you did not.
