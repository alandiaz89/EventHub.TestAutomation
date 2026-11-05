---
name: test-design-techniques
description: Test design knowledge for turning a requirement, endpoint, page or feature into test conditions. Covers black box techniques (equivalence partitioning, boundary value analysis, decision tables, state transitions), white box coverage, experience-based techniques, the functional and non-functional test types, the test-conditions output template, and a safe read-only API probe script. Use when analyzing what to test or when writing a test-conditions file.
argument-hint: <endpoint, feature or requirement>
---

# Test design techniques

Use this to decide **what** to test. It produces test conditions, not test code. For the functional and non-functional test types, read [test-types.md](test-types.md).

## 1. Prepare
1. List every input (query/body fields, headers, auth state, role, UI fields, data state) with type and known rules.
2. List every output (status codes, body fields, messages, side effects).
3. Find out what is already covered: read the client, models, existing tests and `TestCases.md`.
4. Observe real behavior before writing an expected result (see section 6). Do not guess.

## 2. Black box techniques

### Equivalence partitioning (EP)
Split each input into groups the system should treat the same, then take one representative value per group.
- Valid partitions, invalid partitions, plus the special ones: omitted, empty, null, whitespace, wrong type, not in the allowed set, too long.
- One condition per partition. Do not test several values from the same partition unless the first one reveals a bug.

### Boundary value analysis (BVA)
For every ordered range, length or count test the edges and both neighbors:

| Boundary | Values |
|---|---|
| minimum | min-1, min, min+1 |
| maximum | max-1, max, max+1 |

- Two-value BVA tests min and max with the nearest invalid value outside each; three-value BVA adds the value just inside.
- If no maximum is documented, probe upward to see whether one exists, and record the finding.
- Apply it to numbers, string lengths, dates, list sizes, pagination (`page`, `limit`), and quantities (for example tickets 1 and 10, 0 and 11).

### Decision tables
For rules where combinations of conditions give different outcomes (filters that are ANDed, role plus resource, valid/invalid auth plus valid/invalid input):
1. List the conditions and the actions.
2. Write one column per rule (combination).
3. Collapse rules where a condition does not matter, then cover each remaining rule.
For many conditions use pairwise coverage, not the full cross-product.

### State transition
For things with a lifecycle (booking: created, confirmed, cancelled; user: registered, logged in):
- Draw states and events. Cover every valid transition once.
- Cover the important invalid ones (cancel an already cancelled booking; use an expired token).
- Check the side effect of each transition (for example available seats change).

### Use case / scenario
End-to-end flows that connect the pieces: register, log in, browse, book, cancel. Use these for the E2E conditions.

## 3. White box techniques
Only when the code is available (this repository, or code the user points to). The deployed EventHub service is a black box: say so in the output and limit white box conditions to the repository code.
- **Statement coverage:** every line runs at least once.
- **Branch / decision coverage:** each `if`/`else`, `switch` case and default, ternary and early return takes both outcomes.
- **Condition coverage:** each part of a compound condition (`a && b`) is both true and false.
- **Loops:** zero, one, many iterations.
- **Error paths:** every `throw` and every catch.
- Look at how the framework shapes requests too. Examples in this repo: `EventsInput.ToQuery()` (null values are skipped, `CategoryText` overrides `Category`), `ApiClient.ExecuteAsync` (adds the Bearer header only when a token is set), `AuthClient.LoginAndAuthenticate` (throws when the status is not 200 or the token is empty).

## 4. Experience-based techniques
- **Error guessing:** injection strings, very long values, unicode and emoji, leading/trailing whitespace, special characters, case variants, zero and negative numbers, overflow, non-numeric text for numbers, repeated parameters, wrong content type, malformed JSON, empty body.
- **Security and authorization:** missing, malformed and expired tokens; a normal user on admin endpoints; another user's data; ids that do not exist or belong to someone else.
- **Robustness and consistency:** repeated calls (idempotency), concurrency on limited resources (last available seat), totals matching lists, response time sanity.
- **Checklists and exploratory charters:** "Explore X using Y to discover Z", with a time box.

## 5. Output template
Write `test-conditions/<subject>.md` at the repository root. If the file exists, update it and keep existing IDs stable.

```markdown
# Test conditions: <subject>

Subject: ...  | Date: ... | Source: spec / code / observed behavior | Not seen: ...

## Inputs and outputs
| Name | Type | Rules |
|---|---|---|

## <Group, e.g. Authentication>
| ID | Partition / Boundary | Value | Expected |
|---|---|---|---|
| A1 | Valid token | Bearer <valid> | 200, success true |

## Possible product bugs
| ID | Request | Actual | Expected |
|---|---|---|---|

## Coverage notes
- Automated: <condition IDs> -> <test names>
- Not automated: ...
- Recommended first: ...
```

Rules for the table:
- **ID:** a prefix per group plus a number: `A` auth, `P` page, `L` limit, `C` category, `T` text filters, `X` combinations, `W` white box, `E` experience-based, `S` security, `N` non-functional. Never renumber an existing ID.
- **Partition / Boundary:** the condition in words ("Below min (invalid)", "On the boundary, valid", "Branch: token is null").
- **Value:** the concrete input exactly as it would be sent.
- **Expected:** the real numeric status code and the relevant body or behavior, taken from an observed response. Write the status as a number (200, 400, 401, 500). Mark anything you could not verify as "unverified".
- When the observed behavior looks wrong (for example a 500 for bad input), write the correct expected result, flag the row, and list it under Possible product bugs. Never change the expectation to match a defect.
- Mask personal data and never write tokens or passwords.

## 6. Observing the API safely
Use [probe.sh](probe.sh) for read-only `GET` requests. It logs in with the credentials from `src/EventHub.Core/appsettings.local.json` (or the `UserEmail` / `UserPassword` environment variables) and never prints them or the token.

```bash
.claude/skills/test-design-techniques/probe.sh "events?limit=2"
.claude/skills/test-design-techniques/probe.sh "events?page=-1"
.claude/skills/test-design-techniques/probe.sh "auth/me" --no-auth
.claude/skills/test-design-techniques/probe.sh "auth/me" --token invalid.token.value
```

It prints the status code and the JSON body (long bodies are truncated). Do not use it for requests that change data. Ask the user first for anything that creates or deletes data, and clean up only what you created.

## 7. Choosing what to automate
The suite is small, so recommend a proportionate set: all auth conditions, the boundaries that found or could find bugs, one representative per valid partition, and the combinations that matter. Mark exhaustive permutations as not recommended. Say which conditions are data-dependent (they rely on the shared site's seeded data) so they can be written with dynamic expectations.
