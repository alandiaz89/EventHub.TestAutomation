# EventHub API contract

Source: OpenAPI 3.0 spec served by Swagger UI at https://api.eventhub.rahulshettyacademy.com/api/docs/ (spec version 1.0.0, read 2026-11-05). This file is a reference for designing API tests. If the live API and this file disagree, trust the live API and update this file; record the difference under "Observed behavior".

- **Base URL (production):** `https://api.eventhub.rahulshettyacademy.com/api` (configured as `ApiBaseUrl`). The spec also lists `http://localhost:3001/api` for development.
- **Format:** JSON request and response bodies, camelCase fields.
- **Auth:** `Authorization: Bearer <JWT>`. Tokens come from register or login and are valid for 7 days.
- **Sandbox:** each registered user gets an isolated sandbox. Events and bookings are private to that account.
- **Tags:** the spec defines Auth, Events, Bookings and Health. `/config` is tagged `Config`, which the spec does not define in its tag list.

## Endpoint summary

| Method | Path | Auth in spec | Success | Error statuses in spec | Automated here |
|---|---|---|---|---|---|
| POST | `/auth/register` | no | 201 | 400 | yes (`AuthClient.Register`) |
| POST | `/auth/login` | no | 200 | 400, 404 | yes (`AuthClient.Login`) |
| GET | `/auth/me` | bearer | 200 | 401 | yes (`AuthClient.Me`) |
| GET | `/events` | none declared | 200 | 500 | yes (`EventsClient.GetEvents`) |
| POST | `/events` | none declared | 201 | 400, 500 | yes (`CreateEvent`) |
| GET | `/events/{id}` | none declared | 200 | 404, 500 | yes (`GetEventById`) |
| PUT | `/events/{id}` | none declared | 200 | 400, 404, 500 | yes (`UpdateEvent`) |
| DELETE | `/events/{id}` | none declared | 200 | 404, 500 | client method only (`DeleteEvent`); no tests yet |
| GET | `/bookings` | none declared | 200 | 500 | no |
| POST | `/bookings` | none declared | 201 | 400, 404, 500 | no |
| GET | `/bookings/{id}` | none declared | 200 | 404, 500 | no |
| DELETE | `/bookings/{id}` | none declared | 200 | 404, 500 | no |
| GET | `/bookings/ref/{ref}` | none declared | 200 | 404, 500 | no |
| GET | `/health` | no | 200 | none | no |
| GET | `/config` | no | 200 | none | no |

Only `/auth/me` declares `bearerAuth` in the spec, but the tests log in before calling event endpoints. Whether the other endpoints reject a missing token is not stated in the spec; check the live API before writing 401 cases for them.

## Auth

### POST /auth/register
Creates an account and returns a JWT. Body `AuthInput`.
- **201:** `AuthResponse`.
- **400:** validation error (invalid email format, password too short) **or email already registered**. Body is `ValidationErrorResponse` shape: `{ success:false, error:"Validation failed", details:[{field, message}] }`. Example detail: `field: "email"`, `message: "A valid email is required"`.

### POST /auth/login
Body `AuthInput`.
- **200:** `AuthResponse`.
- **400:** wrong password or validation error. Example: `{ success:false, error:"Invalid credentials" }`.
- **404:** no account for the email. Example: `{ success:false, error:"User not found" }`.

### GET /auth/me
Validates the bearer token and returns the identity decoded from it.
- **200:** `MeResponse`.
- **401:** missing or invalid token. Example: `{ success:false, error:"Unauthorized" }`.

## Events

### GET /events
Paginated list. Query parameters:

| Name | Type | Notes |
|---|---|---|
| `category` | string enum | `Conference`, `Concert`, `Sports`, `Workshop`, `Festival` |
| `city` | string | example `Bangalore` |
| `search` | string | matches title, description or venue |
| `page` | integer | default 1, minimum 1 |
| `limit` | integer | default 10, minimum 1, maximum 100 |

- **200:** `{ success, data: Event[], pagination: PaginationMeta }`.
- **500:** `ErrorResponse`.

### POST /events
Creates an event; `availableSeats` is set equal to `totalSeats`. Body `CreateEventInput`.
- **201:** `{ success:true, data: Event, message:"Event created successfully" }`.
- **400:** `ValidationErrorResponse`.
- **500:** `ErrorResponse`.

### GET /events/{id}
`id` is an integer.
- **200:** `{ success, data: Event }`.
- **404:** `{ success:false, error:"Event with id 99 not found" }`.
- **500:** `ErrorResponse`.

### PUT /events/{id}
Body `CreateEventInput` (full replacement: every required field must be sent, including `title`).
- **200:** `{ success:true, data: Event, message:"Event updated successfully" }`.
- **400:** `ValidationErrorResponse`.
- **404:** `ErrorResponse`.
- **500:** `ErrorResponse`.

### DELETE /events/{id}
Permanently deletes the event **and all its bookings (cascade)**.
- **200:** `{ success:true, message:"Event deleted successfully" }`.
- **404:** `ErrorResponse`.
- **500:** `ErrorResponse`.

## Bookings

### GET /bookings
Paginated list of bookings, each including full event details. Query parameters:

| Name | Type | Notes |
|---|---|---|
| `eventId` | integer | filter by event |
| `status` | string enum | `confirmed`, `cancelled` |
| `page` | integer | default 1, minimum 1 |
| `limit` | integer | default 10, minimum 1, maximum 100 |

- **200:** `{ success, data: Booking[], pagination: PaginationMeta }`.
- **500:** `ErrorResponse`.

### POST /bookings
Body `CreateBookingInput`. The service: verifies the event exists, checks enough seats are available, calculates the total price, generates a unique reference (`EVT-XXXXXX`), and atomically creates the booking and decrements `availableSeats`.
- **201:** `{ success:true, data: Booking, message:"Booking confirmed!" }`.
- **400:** either `ValidationErrorResponse` (example: `field: "quantity"`, `message: "Quantity must be an integer between 1 and 10"`) or `ErrorResponse` for insufficient seats (example: `"Only 1 seat(s) available, but 3 requested"`).
- **404:** event not found (`ErrorResponse`).
- **500:** `ErrorResponse`.

### GET /bookings/{id}
`id` is an integer.
- **200:** `{ success, data: Booking }`.
- **404 / 500:** `ErrorResponse`.

### GET /bookings/ref/{ref}
`ref` is the booking reference, example `EVT-A1B2C3`.
- **200:** `{ success, data: Booking }`.
- **404:** example `{ success:false, error:"Booking with reference \"EVT-XYZ123\" not found" }`.
- **500:** `ErrorResponse`.

### DELETE /bookings/{id}
Cancels (permanently deletes) the booking and atomically restores the seats to the event's `availableSeats`.
- **200:** `{ success:true, message:"Booking cancelled" }`.
- **404 / 500:** `ErrorResponse`.

## Health and config

- `GET /health` -> 200 `{ status:"ok", timestamp: date-time, dbStatus:"connected" }`.
- `GET /config` -> 200 `{ showExploreLinks: boolean }` (public feature flags; example `false`).

## Schemas

### AuthInput (request; both fields required)
| Field | Type | Rules |
|---|---|---|
| `email` | string | format email |
| `password` | string | minLength 6 |

### AuthResponse
| Field | Type | Notes |
|---|---|---|
| `success` | boolean | |
| `token` | string | JWT, valid 7 days |
| `user.id` | integer | |
| `user.email` | string | |

### MeResponse
| Field | Type | Notes |
|---|---|---|
| `success` | boolean | |
| `user.userId` | integer | note: `userId` here, `id` in AuthResponse |
| `user.email` | string | |

### Event
| Field | Type | Notes |
|---|---|---|
| `id` | integer | |
| `title` | string | |
| `description` | string | |
| `category` | string | |
| `venue` | string | |
| `city` | string | |
| `eventDate` | string | date-time, ISO 8601 UTC, example `2026-06-15T09:00:00.000Z` |
| `price` | number (float) | see "Observed behavior": returned as a string |
| `totalSeats` | integer | |
| `availableSeats` | integer | |
| `imageUrl` | string, nullable | |
| `createdAt` | string | date-time |
| `updatedAt` | string | date-time |

### CreateEventInput (request)
Required: `title`, `category`, `venue`, `city`, `eventDate`, `price`, `totalSeats`. Optional: `description`, `imageUrl`.

| Field | Type | Notes |
|---|---|---|
| `title` | string | |
| `description` | string | optional |
| `category` | string | the spec types it as a plain string; the `/events` filter enum lists `Conference`, `Concert`, `Sports`, `Workshop`, `Festival` |
| `venue` | string | |
| `city` | string | |
| `eventDate` | string | date-time |
| `price` | number | |
| `totalSeats` | integer | |
| `imageUrl` | string | optional |

The spec states no min/max/length rules for these fields. Limits must be found by probing (see "Observed behavior").

### Booking
| Field | Type | Notes |
|---|---|---|
| `id` | integer | |
| `eventId` | integer | |
| `customerName` | string | |
| `customerEmail` | string | |
| `customerPhone` | string | |
| `quantity` | integer | |
| `totalPrice` | number (float) | |
| `status` | string | `confirmed` or `cancelled` |
| `bookingRef` | string | format `EVT-XXXXXX` |
| `createdAt` / `updatedAt` | string | date-time |
| `event` | Event | full event details |

### CreateBookingInput (request; all fields required)
| Field | Type | Rules |
|---|---|---|
| `eventId` | integer | |
| `customerName` | string | minLength 2 |
| `customerEmail` | string | format email |
| `customerPhone` | string | minLength 10 |
| `quantity` | integer | 1 to 10 inclusive |

### PaginationMeta
`total`, `page`, `limit`, `totalPages` (all integers).

### ErrorResponse
`{ success: false, error: string }`

### ValidationErrorResponse
`{ success: false, error: "Validation failed", details: [{ field: string, message: string }] }`

## Observed behavior (live API vs spec)

Sources: the `[Category("Product-Bug")]` tests in EventHub.ApiTests, their code comments, and the test run of 2026-11-05 in which all 11 of them failed. CI can exclude them with `--filter "Category!=Product-Bug"`.

The "Expected by the test" column is the test author's expectation. It is **not** always stated in the spec, so the spec column says what the spec actually declares.

| Request | Spec declares | Expected by the test | Live API (observed) |
|---|---|---|---|
| Any event response, field `price` | `number` | number | JSON string, for example `"150"` (test comment) |
| `GET /events?page=-1` | `page` has `minimum: 1`; responses 200, 500 | 400 | 500 |
| `GET /events/{id}` with a non-numeric id (`abc`) or a number followed by text (`1abc`) | `id` is `integer`; responses 200, 404, 500 | 400 | 500 |
| `GET /events/{id}` with a very large id | responses 200, 404, 500 | 404 | 500 |
| `POST /events` with a 1000-character title | responses 201, 400, 500; no title length limit stated | 400 | 500 `Internal server error` (test comment: 200 characters already fail, 150 work) |
| `POST /events` with malformed JSON | responses 201, 400, 500 | 400 | 500 |
| `POST /events` twice with the same title as the same user | 201, 400, 500; no upsert described | the first event keeps its data | 201 both times, same event id, the second post overwrites the first (test comment) |
| `GET /events/{id}` response data | `Event` schema has no `userId` or `isStatic` | neither field present | both fields present (test comment) |

Note that for the 500 rows the spec does list 500 as a possible response for those endpoints, but only as a generic "Internal server error". It does not say these inputs should produce it.

## Using this file for test design

- Booking tests are the biggest gap: no client, models or tests exist yet. Needed: `BookingsClient`, `CreateBookingInput`/`Booking` models and a builder. Cases to cover: quantity boundaries 0, 1, 10, 11; insufficient seats (400); unknown event (404); seats decrease on booking and are restored on cancel; `bookingRef` format; filtering by `eventId` and `status`; lookup by id and by reference; cascade delete of bookings when the event is deleted.
- Event delete (`DeleteEventTests`) is empty: 200, 404 on second delete, event no longer retrievable, 404 for an unknown id.
- Any endpoint where the spec does not declare auth: probe the live API with no token and with an invalid token before asserting a status.
- Take every expected status code from a real response, and cross-check it with this file, per the `api-testcase-designer` agent rules.
