# CLAUDE.md (Database)

## Common commands

Scripts are meant to be run in order against an Oracle instance (schema `APPUSER`), via `run_all.sql`:
1. `05_Cleanup_Unused_Objects.sql` (drop old objects)
2. `01_Tables.sql` (tables, sequences, default admin row)
3. `02_Triggers.sql` (audit-history triggers + helper function)
4. `03_Procedures.sql` (PKG_MUSTERI / PKG_HESAP / PKG_HESAPHAREKET / PKG_DASHBOARD packages)
5. `04_Examples.sql` (sample data / usage)

`00_Full_Reset.sql` tears everything down for a clean re-run. Connection string (dev) is in `Backend/BankAPI/appsettings.json` (`ConnectionStrings:OracleConnection`, Oracle XE at `localhost:1521/XEPDB1`, user `appuser`).

## Database architecture

- Table prefixes indicate the subject area: `MST_*` (master data: customers, addresses), `MVD_*` (movement/transactional data: accounts, transactions, admin, daily volume).
- Every core table has a matching `*_H` history table (`MST_MUSTERI_H`, `MVD_HESAP_H`, etc.) populated by `BEFORE INSERT OR UPDATE` triggers in `02_Triggers.sql`, driven by `FN_AKTIF_ADMIN_KULLANICI_ADI` to stamp who made the change. When adding a new table that needs auditing, mirror this pattern (history table + trigger), don't bolt on application-level logging instead.
- `MVD_HESAPHAREKET.ISLEM_YONU` is `'B'`/`'C'` (borç/alacak — debit/credit); `MST_MUSTERI.MUSTERI_TIPI` is `1` (bireysel/individual) or `2` (tüzel/corporate); `AKTIF_MI`/`DURUM` fields use small integer codes rather than booleans — check the `CHECK` constraints in `01_Tables.sql` for the valid values before writing new procedures.
- PL/SQL packages in `03_Procedures.sql` are the source of truth for business logic (e.g. account balance updates on transaction insert). If a bug looks like "wrong balance" or "wrong customer count," check the procedure body before touching C#.
