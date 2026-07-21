ALTER SESSION SET CURRENT_SCHEMA = APPUSER;

-- 1. Temizlik
@DB\05_Cleanup_Unused_Objects.sql

-- 2. Tablolari ve iliskili nesneleri olustur
@DB\01_Tables.sql

-- 3. Trigger ve package/prosedurleri derle
@DB\02_Triggers.sql
@DB\03_Procedures.sql

-- 4. Ornek calistirma adimlari
@DB\04_Examples.sql

EXIT;
