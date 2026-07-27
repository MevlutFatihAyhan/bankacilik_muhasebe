# CLAUDE.md (Frontend)

## Common commands

```
npm install
npm start        # ng serve, http://localhost:4200
npm run build    # ng build -> dist/bank-frontend
npm test         # ng test (Karma/Jasmine)
```
To run a single spec file, use Karma's `--include` or filter in the browser test runner launched by `ng test`; there's no dedicated single-test CLI flag configured beyond Angular CLI defaults.

## Frontend architecture

- Angular 18, **standalone components** (no NgModules) — see `app.config.ts` for global providers (`provideRouter`, `provideHttpClient(withFetch())`).
- Routing (`app.routes.ts`) has two top-level areas: `/login` and `/admin` (lazy-loaded children under `admin` for each page — customer list/detail/add, account list/detail/add, transaction list/detail). Add new admin pages as lazy `loadComponent` children here, following the existing naming (`xxx-listesi` = list, `xxx-detayi` = detail, `xxx-ekle` = add).
- Each backend domain has a matching Angular service in `src/app/services/*.service.ts` (e.g. `musteri.service.ts`, `hesap.service.ts`, `hesap-hareket.service.ts`, `adres.service.ts`) that calls a hardcoded `http://localhost:5064/api/...` base URL and returns `Observable`s — no environment-based API URL config yet, so update the string in each service if the backend URL changes.
- Models under `src/app/models/*.model.ts` mirror the backend DTOs.
- Custom pipes (`filter.pipe.ts`, `sort.pipe.ts`) are used for in-template list filtering/sorting instead of doing it in the component.
- The login page (`pages/login`) is currently a static component with no auth logic wired to the backend — `MVD_ADMIN` exists in the DB but there is no login/auth controller or service yet.
