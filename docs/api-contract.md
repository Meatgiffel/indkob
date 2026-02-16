# API Contract

Reviewed on: 2026-02-16

This document describes the HTTP contract implemented by the API in `Api/Controllers`.

## Base and Conventions
- Base path: `/api`
- Auth model: cookie session (`indkob.auth`)
- API style: JSON request/response, `ProblemDetails`/`ValidationProblemDetails` for many errors
- Date format: ISO date strings (`yyyy-MM-dd`) for `DateOnly` values

## Authentication and Authorization
- Cookie scheme: `IndkobCookie` (`Api/Auth/AuthConstants.cs`)
- Admin claim: `indkob:isAdmin=true`
- Unauthorized responses return:
  - `401` for unauthenticated requests
  - `403` for authenticated users without required policy
- Protected controllers:
  - Any authenticated user: `ItemsController`, `GroceryEntriesController`, `MealPlanController`
  - Admin only: `UsersController`

## DTOs

### Auth
- `LoginRequest`
  - `userName: string`
  - `password: string`
  - `rememberMe: boolean` (optional, default `false`)
- `AuthUserDto`
  - `id: number`
  - `userName: string`
  - `isAdmin: boolean`

### Items
- `ItemDto`
  - `id: number`
  - `name: string`
  - `area: string`
- `CreateItemRequest` / `UpdateItemRequest`
  - `name: string` (required, max 128)
  - `area: string` (required, max 128)

### Grocery Entries
- `GroceryEntryDto`
  - `id: number`
  - `itemId: number | null`
  - `itemName: string | null`
  - `itemArea: string | null`
  - `amount: string | null` (max 64)
  - `note: string | null` (max 512)
  - `isDone: boolean`
  - `createdAt: string` (ISO datetime)
- `CreateGroceryEntryRequest`
  - `itemId: number | null` (if present, must be >= 1)
  - `amount: string | null` (max 64)
  - `note: string | null` (max 512)
- `UpdateGroceryEntryRequest`
  - Same as create + `isDone: boolean`

### Meal Plan
- `MealPlanDayDto`
  - `date: string` (`yyyy-MM-dd`)
  - `dinner: string | null`
- `UpdateMealPlanDayRequest`
  - `dinner: string | null`

### Users
- `UserDto`
  - `id: number`
  - `userName: string`
  - `isAdmin: boolean`
  - `createdAt: string` (ISO datetime)
- `CreateUserRequest`
  - `userName: string`
  - `password: string` (min 6)
  - `isAdmin: boolean`
- `UpdateUserRequest`
  - `userName: string`
  - `password: string | null` (optional; if provided min 6)
  - `isAdmin: boolean`

## Endpoints

### Auth

`POST /api/auth/login`
- Auth: anonymous allowed
- Body: `LoginRequest`
- Success: `200 OK` with `AuthUserDto`, sets auth cookie
- Errors:
  - `401 Unauthorized` for missing credentials, unknown username, or wrong password

`POST /api/auth/logout`
- Auth: not required
- Body: empty object accepted
- Success: `204 No Content`, clears auth cookie if present

`GET /api/auth/me`
- Auth: not required at attribute level, but endpoint returns user only when authenticated
- Success: `200 OK` with `AuthUserDto`
- Errors:
  - `401 Unauthorized` if not authenticated or claims are invalid

### Items

`GET /api/items`
- Auth: required
- Success: `200 OK` with `ItemDto[]`
- Ordering: by `area`, then `name`

`GET /api/items/{id}`
- Auth: required
- Success: `200 OK` with `ItemDto`
- Errors:
  - `404 Not Found` if item does not exist

`POST /api/items`
- Auth: required
- Body: `CreateItemRequest`
- Success: `201 Created` with `ItemDto` (`Location` header points to `GET /api/items/{id}`)
- Errors:
  - `400 Bad Request` for missing/invalid `name` or `area`
  - `409 Conflict` if an item with same case-insensitive name already exists

`PUT /api/items/{id}`
- Auth: required
- Body: `UpdateItemRequest`
- Success: `200 OK` with updated `ItemDto`
- Errors:
  - `400 Bad Request` for missing/invalid `name` or `area`
  - `404 Not Found` if item does not exist
  - `409 Conflict` if another item has same case-insensitive name

`DELETE /api/items/{id}`
- Auth: required
- Success: `204 No Content`
- Errors:
  - `404 Not Found` if item does not exist
  - `409 Conflict` if item is referenced by any grocery entry

### Grocery Entries

`GET /api/groceryentries`
- Auth: required
- Success: `200 OK` with `GroceryEntryDto[]`
- Ordering:
  - open entries before done entries
  - grouped/sorted by area (`da-DK` comparer), notes use area `Noter`

`GET /api/groceryentries/{id}`
- Auth: required
- Success: `200 OK` with `GroceryEntryDto`
- Errors:
  - `404 Not Found` if entry does not exist

`POST /api/groceryentries`
- Auth: required
- Body: `CreateGroceryEntryRequest`
- Success: `201 Created` with `GroceryEntryDto`
- Rules:
  - If `itemId` is `null`, `note` is required (note-only entry)
  - If `itemId` is provided, referenced item must exist
- Errors:
  - `400 Bad Request` for validation failures (including missing note for note-only entry)
  - `404 Not Found` when referenced item does not exist

`PUT /api/groceryentries/{id}`
- Auth: required
- Body: `UpdateGroceryEntryRequest`
- Success: `200 OK` with updated `GroceryEntryDto`
- Rules:
  - If `itemId` is `null`, `note` is required
  - If `itemId` is provided, referenced item must exist
- Errors:
  - `400 Bad Request` for validation failures
  - `404 Not Found` if entry does not exist or referenced item does not exist

`DELETE /api/groceryentries/{id}`
- Auth: required
- Success: `204 No Content`
- Errors:
  - `404 Not Found` if entry does not exist

`POST /api/groceryentries/clear`
- Auth: required
- Body: empty object accepted
- Success: `204 No Content` (deletes all grocery entries)

### Meal Plan

`GET /api/mealplan?weekStart=yyyy-MM-dd`
- Auth: required
- Query:
  - `weekStart` (required `DateOnly`)
- Success: `200 OK` with exactly 7 `MealPlanDayDto` values (`weekStart` through `weekStart + 6`)
- Notes:
  - Server does not require `weekStart` to be Monday; client sends Monday

`PUT /api/mealplan/{date}`
- Auth: required
- Route:
  - `date` (`DateOnly`)
- Body: `UpdateMealPlanDayRequest`
- Success: `200 OK` with `MealPlanDayDto`
- Behavior:
  - blank/whitespace dinner deletes stored entry for that date and returns `dinner: null`
  - non-empty dinner upserts entry
- Errors:
  - `400 Bad Request` if dinner exceeds 256 chars or date format is invalid

### Users (Admin)

`GET /api/users`
- Auth: admin policy required
- Success: `200 OK` with `UserDto[]`
- Ordering: by `userName`

`POST /api/users`
- Auth: admin policy required
- Body: `CreateUserRequest`
- Success: `200 OK` with `UserDto`
- Errors:
  - `400 Bad Request` if username/password missing or password shorter than 6
  - `409 Conflict` if username already exists (case-insensitive)

`PUT /api/users/{id}`
- Auth: admin policy required
- Body: `UpdateUserRequest`
- Success: `200 OK` with updated `UserDto`
- Errors:
  - `400 Bad Request` if username missing or password shorter than 6 when provided
  - `404 Not Found` if user does not exist
  - `409 Conflict` if username already exists on another user

`DELETE /api/users/{id}`
- Auth: admin policy required
- Success: `204 No Content`
- Errors:
  - `404 Not Found` if user does not exist
  - `409 Conflict` if trying to delete own logged-in user
  - `409 Conflict` if trying to delete the last admin user

## SignalR Realtime Contract

### Hub Endpoint

`GET/WS /api/hubs/grocery`
- Auth: required (same cookie session as HTTP API)
- Client transport: LongPolling (forced for proxy compatibility and predictable reconnect behavior)
- Compatibility path: `/hubs/grocery` is also mapped server-side for older clients

### Server-to-Client Event

`groceryChanged`
- Payload:
  - `type: 'created' | 'updated' | 'deleted' | 'cleared'`
  - `entryId: number | null`
  - `atUtc: string` (ISO datetime)

Event semantics:
- `created`: a grocery entry was added
- `updated`: a grocery entry changed (including `isDone` toggles)
- `deleted`: one grocery entry removed
- `cleared`: entire grocery list removed
