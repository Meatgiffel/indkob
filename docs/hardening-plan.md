# Hardening Plan

Reviewed on: 2026-02-16

This plan is prioritized from highest risk to lowest, based on the current implementation in `Api/` and `client/`.

## Priority Scale
- `P0`: should be done first (security/data-integrity exposure)
- `P1`: important next (security posture and reliability)
- `P2`: medium-term improvements

## P0 - Immediate

### 1) Remove default bootstrap credentials and password logging
- Current state:
  - If no users exist, bootstrap user defaults to `admin/changeme`.
  - Startup log includes the bootstrap password.
- Risk:
  - Predictable credentials and plaintext password exposure in logs.
- Actions:
  - In production, require explicit `Auth__BootstrapUser` and `Auth__BootstrapPassword` or fail startup.
  - Remove password from logs.
  - Optionally allow bootstrap only on first deployment window, then disable.
- Acceptance criteria:
  - No plaintext passwords in logs.
  - Production cannot start with implicit default admin credentials.

### 2) Enforce strict CORS configuration in production
- Current state:
  - Missing `Cors:AllowedOrigins` falls back to `AllowAnyOrigin`.
- Risk:
  - Insecure default if config is missing or broken.
- Actions:
  - Fail startup in production when allowed origins are empty.
  - Keep permissive fallback only for explicit local/dev environments.
- Acceptance criteria:
  - Production requires non-empty allowlist and refuses startup otherwise.

### 3) Add login brute-force protection
- Current state:
  - `/api/auth/login` has no rate limiting or lockout policy.
- Risk:
  - Password-guessing attacks.
- Actions:
  - Add ASP.NET rate limiting on login endpoint (per IP + per username bucket).
  - Add temporary lockout/backoff after repeated failures.
- Acceptance criteria:
  - Repeated failed attempts are throttled and logged.

### 4) Enforce item uniqueness at database level
- Current state:
  - Duplicate prevention is app-level (`ToLower()` checks), no DB unique index.
- Risk:
  - Race condition can create duplicates under concurrent writes.
- Actions:
  - Add normalized item name column (or computed equivalent) with unique index.
  - Keep API-level conflict checks for user-friendly messages.
- Acceptance criteria:
  - DB enforces uniqueness for item names case-insensitively.

## P1 - Next

### 5) Tighten transport and cookie security
- Current state:
  - Cookie uses `SecurePolicy = SameAsRequest`; HTTPS redirection is config-gated.
- Risk:
  - Misconfiguration can allow insecure transport/cookie behavior.
- Actions:
  - Enforce HTTPS externally.
  - Use `CookieSecurePolicy.Always` in production.
  - Enable HSTS where appropriate.
- Acceptance criteria:
  - Auth cookie is always secure in production.
  - HTTP access is redirected or blocked as intended.

### 6) Add CSRF protection strategy for cookie-authenticated writes
- Current state:
  - Session auth uses cookies for state-changing endpoints.
- Risk:
  - Cross-site request risks increase if same-site/cors protections drift.
- Actions:
  - Implement anti-forgery token strategy (double-submit or framework anti-forgery).
  - Document required client behavior.
- Acceptance criteria:
  - Write endpoints reject requests without valid anti-forgery protection.

### 7) Strengthen audit logging for sensitive operations
- Current state:
  - No structured audit events for admin/destructive actions.
- Risk:
  - Low traceability for incidents and operational debugging.
- Actions:
  - Log structured events for login success/fail, user CRUD, and list clear.
  - Exclude secrets and personal sensitive values.
- Acceptance criteria:
  - Security-relevant actions are traceable by user/time/result.

### 8) Add API integration tests for critical behavior
- Current state:
  - No automated API tests.
- Risk:
  - Auth/authorization and business-rule regressions during refactors.
- Actions:
  - Add integration tests for:
    - auth lifecycle (`login`, `me`, `logout`)
    - admin-only user endpoints
    - note-only grocery entries
    - last-admin and self-delete protections
    - meal plan upsert/delete-by-empty behavior
- Acceptance criteria:
  - CI runs these tests and blocks merges on failure.

### 9) Add explicit length/format limits for user fields
- Current state:
  - `UserName` has required checks but no explicit max-length constraint in model configuration.
- Risk:
  - Unbounded user input can create operational and indexing issues.
- Actions:
  - Add max lengths and matching DTO validation.
  - Align API validation with DB schema.
- Acceptance criteria:
  - User fields have consistent max lengths in DTO + EF model + DB schema.

## P2 - Medium Term

### 10) Dependency and vulnerability management
- Current state:
  - Frontend dependency tree currently reports known vulnerabilities (`npm audit`).
- Actions:
  - Add dependency update cadence and vulnerability triage process.
  - Patch high-severity issues where compatible.
- Acceptance criteria:
  - Regular security updates and documented exceptions.

### 11) Clean stale test/docs artifacts and enforce doc drift checks
- Current state:
  - `client/src/app/app.component.spec.ts` and `Api/Api.http` are stale.
- Actions:
  - Replace stale samples/tests with current app behavior.
  - Add CI checks or checklist to keep docs/tests aligned with routes/controllers.
- Acceptance criteria:
  - No legacy scaffold references in tests/docs.

### 12) Improve backup and disaster recovery for SQLite deployments
- Current state:
  - SQLite is persisted, but backup/restore process is not documented in depth.
- Actions:
  - Document scheduled backup/restore and retention.
  - Test restore procedure in staging.
- Acceptance criteria:
  - Verified recovery runbook with tested restore steps.
