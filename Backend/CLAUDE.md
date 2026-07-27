# CLAUDE.md (Backend)

## Common commands

```
dotnet build              # build
dotnet run                # run API (http://localhost:5064, https://localhost:7270)
```
Swagger UI is available at `/swagger` when running in Development. There are no backend test projects in this repo currently.

## Backend architecture

Every domain has the same three-layer shape — follow it exactly when adding a new one:

1. **Model** (`Models/*.cs`) — plain DTO matching the Oracle table shape (e.g. `Musteri.cs` ↔ `MST_MUSTERI`). Field names generally mirror the Oracle column names; `[JsonPropertyName]` is used where the C# property name needs to differ from the wire format (e.g. `MusteriID` → `musteriId`).
2. **Service** (`Services/*.cs`) — registered as `Scoped` in `Program.cs`. Opens an `OracleConnection` per call, invokes a stored procedure via `OracleCommand` with `CommandType.StoredProcedure` and `BindByName = true`, and manually maps `OracleDataReader` rows to models. There is no ORM/EF Core — everything is raw ADO.NET against Oracle packages (`PKG_MUSTERI.PRC_...`, `PKG_HESAP.PRC_...`, etc.). List-returning procs use an output `RefCursor` parameter; multi-result-set procs (see `DashboardService`) are read with successive `reader.NextResult()` calls, one per cursor, in the fixed order the PL/SQL procedure returns them.
3. **Controller** (`Controllers/*Controller.cs`) — `[ApiController]`, `[Route("api/[controller]")]`. Every action wraps the service call in try/catch and returns `StatusCode(500, new { message = ... })` on failure — follow this pattern for consistency rather than introducing global exception middleware.

Services are constructed with `IConfiguration` and read `ConnectionStrings:OracleConnection` directly — there's no repository/unit-of-work abstraction beyond this.

CORS is locked to a single named policy `AngularProject` allowing only `http://localhost:4200` (see `Program.cs`); update that origin if the frontend's dev URL changes.

Domain naming is Turkish throughout (Müşteri = Customer, Hesap = Account, Hareket = Transaction/movement, Adres = Address) — keep new code consistent with this naming rather than mixing in English terms.
