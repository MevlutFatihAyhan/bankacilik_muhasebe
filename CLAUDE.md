# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

A Turkish banking/accounting demo app (bankacılık muhasebe) with three layers:

- **`DB/`** — Oracle schema: tables, triggers, and PL/SQL packages. This is where the actual business logic lives.
- **`Backend/BankAPI/`** — ASP.NET Core (net10.0) Web API that is a thin pass-through to Oracle stored procedures.
- **`Frontend/`** — Angular 18 standalone-component SPA (admin panel style UI).

The backend does **not** contain business rules — validation, ID generation, balance updates, and audit history are all done in the database layer (triggers + PL/SQL packages under `PKG_MUSTERI`, `PKG_HESAP`, `PKG_HESAPHAREKET`, `PKG_DASHBOARD`). When changing behavior (e.g. account balance calculation, customer validation), look in `DB/03_Procedures.sql` and `DB/02_Triggers.sql` first, not just the C# service.

## Common commands

### Backend (`Backend/BankAPI`)
```
dotnet build              # build
dotnet run                # run API (http://localhost:5064, https://localhost:7270)
```
Swagger UI is available at `/swagger` when running in Development. There are no backend test projects in this repo currently.

### Frontend (`Frontend/`)
```
npm install
npm start        # ng serve, http://localhost:4200
npm run build    # ng build -> dist/bank-frontend
npm test         # ng test (Karma/Jasmine)
```
To run a single spec file, use Karma's `--include` or filter in the browser test runner launched by `ng test`; there's no dedicated single-test CLI flag configured beyond Angular CLI defaults.

### Database (`DB/`)
Scripts are meant to be run in order against an Oracle instance (schema `APPUSER`), via `run_all.sql`:
1. `05_Cleanup_Unused_Objects.sql` (drop old objects)
2. `01_Tables.sql` (tables, sequences, default admin row)
3. `02_Triggers.sql` (audit-history triggers + helper function)
4. `03_Procedures.sql` (PKG_MUSTERI / PKG_HESAP / PKG_HESAPHAREKET / PKG_DASHBOARD packages)
5. `04_Examples.sql` (sample data / usage)

`00_Full_Reset.sql` tears everything down for a clean re-run. Connection string (dev) is in `Backend/BankAPI/appsettings.json` (`ConnectionStrings:OracleConnection`, Oracle XE at `localhost:1521/XEPDB1`, user `appuser`).

## Backend architecture

Every domain has the same three-layer shape — follow it exactly when adding a new one:

1. **Model** (`Models/*.cs`) — plain DTO matching the Oracle table shape (e.g. `Musteri.cs` ↔ `MST_MUSTERI`). Field names generally mirror the Oracle column names; `[JsonPropertyName]` is used where the C# property name needs to differ from the wire format (e.g. `MusteriID` → `musteriId`).
2. **Service** (`Services/*.cs`) — registered as `Scoped` in `Program.cs`. Opens an `OracleConnection` per call, invokes a stored procedure via `OracleCommand` with `CommandType.StoredProcedure` and `BindByName = true`, and manually maps `OracleDataReader` rows to models. There is no ORM/EF Core — everything is raw ADO.NET against Oracle packages (`PKG_MUSTERI.PRC_...`, `PKG_HESAP.PRC_...`, etc.). List-returning procs use an output `RefCursor` parameter; multi-result-set procs (see `DashboardService`) are read with successive `reader.NextResult()` calls, one per cursor, in the fixed order the PL/SQL procedure returns them.
3. **Controller** (`Controllers/*Controller.cs`) — `[ApiController]`, `[Route("api/[controller]")]`. Every action wraps the service call in try/catch and returns `StatusCode(500, new { message = ... })` on failure — follow this pattern for consistency rather than introducing global exception middleware.

Services are constructed with `IConfiguration` and read `ConnectionStrings:OracleConnection` directly — there's no repository/unit-of-work abstraction beyond this.

CORS is locked to a single named policy `AngularProject` allowing only `http://localhost:4200` (see `Program.cs`); update that origin if the frontend's dev URL changes.

Domain naming is Turkish throughout (Müşteri = Customer, Hesap = Account, Hareket = Transaction/movement, Adres = Address) — keep new code consistent with this naming rather than mixing in English terms.

## Database architecture

- Table prefixes indicate the subject area: `MST_*` (master data: customers, addresses), `MVD_*` (movement/transactional data: accounts, transactions, admin, daily volume).
- Every core table has a matching `*_H` history table (`MST_MUSTERI_H`, `MVD_HESAP_H`, etc.) populated by `BEFORE INSERT OR UPDATE` triggers in `02_Triggers.sql`, driven by `FN_AKTIF_ADMIN_KULLANICI_ADI` to stamp who made the change. When adding a new table that needs auditing, mirror this pattern (history table + trigger), don't bolt on application-level logging instead.
- `MVD_HESAPHAREKET.ISLEM_YONU` is `'B'`/`'C'` (borç/alacak — debit/credit); `MST_MUSTERI.MUSTERI_TIPI` is `1` (bireysel/individual) or `2` (tüzel/corporate); `AKTIF_MI`/`DURUM` fields use small integer codes rather than booleans — check the `CHECK` constraints in `01_Tables.sql` for the valid values before writing new procedures.
- PL/SQL packages in `03_Procedures.sql` are the source of truth for business logic (e.g. account balance updates on transaction insert). If a bug looks like "wrong balance" or "wrong customer count," check the procedure body before touching C#.

## Frontend architecture

- Angular 18, **standalone components** (no NgModules) — see `app.config.ts` for global providers (`provideRouter`, `provideHttpClient(withFetch())`).
- Routing (`app.routes.ts`) has two top-level areas: `/login` and `/admin` (lazy-loaded children under `admin` for each page — customer list/detail/add, account list/detail/add, transaction list/detail). Add new admin pages as lazy `loadComponent` children here, following the existing naming (`xxx-listesi` = list, `xxx-detayi` = detail, `xxx-ekle` = add).
- Each backend domain has a matching Angular service in `src/app/services/*.service.ts` (e.g. `musteri.service.ts`, `hesap.service.ts`, `hesap-hareket.service.ts`, `adres.service.ts`) that calls a hardcoded `http://localhost:5064/api/...` base URL and returns `Observable`s — no environment-based API URL config yet, so update the string in each service if the backend URL changes.
- Models under `src/app/models/*.model.ts` mirror the backend DTOs.
- Custom pipes (`filter.pipe.ts`, `sort.pipe.ts`) are used for in-template list filtering/sorting instead of doing it in the component.
- The login page (`pages/login`) is currently a static component with no auth logic wired to the backend — `MVD_ADMIN` exists in the DB but there is no login/auth controller or service yet.
