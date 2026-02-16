# Repository Guidelines

## Project Structure & Modules
- `client/`: Angular standalone app with PrimeNG/PrimeFlex UI. Entry `src/main.ts`; shell/layout in `src/app/app.component.*`; pages `login-page`, `list-page`, `shop-page`, `meal-plan-page`, `items-page`, and admin `users-page`. Global styles in `src/styles.scss`. Assets in `src/assets/` (logo, icons).
- `Api/`: ASP.NET Core (.NET 10) API with SQLite; entry `Api/Program.cs`; data in `Api/Data`, controllers in `Api/Controllers`, DTOs in `Api/Dtos`, and migrations in `Api/Migrations`.
- Root: repo-level `.gitignore`, docs (`README.md`, `AGENTS.md`, `docs/api-contract.md`, `docs/hardening-plan.md`).

## Build, Test, Dev Commands
- Client dev: `cd client && npm install && npm start` (ng serve, defaults to 4200).
- Client build: `npm run build` (outputs to `client/dist/client`).
- API dev: `cd Api && dotnet restore && dotnet run` (check `launchSettings.json` for port).
- API DB reset: `cd Api && dotnet ef database update`.

## Coding Style & Naming
- Angular: standalone components, SCSS; prefer descriptive names (`list-page.component.*`, `items-page.component.*`). Keep UI compact and mobile-friendly; avoid new external fonts (offline build).
- .NET: follow controller/API patterns already present; PascalCase for public members.
- Indentation: 2 spaces (TS/HTML/SCSS), default C# formatting in API.

## Testing Guidelines
- No frontend/unit test harness currently in use; manual `npm run build` as smoke check.
- API uses EF migrations; no automated tests present. If adding tests, place under appropriate project with standard `dotnet test`.

## Commit & PR Guidelines
- Use clear, action-oriented commit messages (e.g., `Improve search create flow`, `Add toolbar logo`).
- PRs: include summary of UI/UX changes, mention build/test status (`npm run build`, API run if touched), and screenshots/GIFs for UI when practical. Link issues if applicable.

## Security & Configuration
- API DB is SQLite; avoid committing secrets. Base URL is read from `client/src/environments/environment.ts`.
- SQLite files under `Api/` are gitignored (`Api/*.db`); use migrations to recreate local DB.
- Keep `client/src/assets/logo.png` referenced in toolbar, manifest, and favicon. Do not remove without replacing.

## Agent Notes
- Search/add flow: autocomplete allows “Opret …” option at end of suggestions and Enter-to-create when no selection; uses typed term and area suggestion. Preserve this behavior when refactoring.
