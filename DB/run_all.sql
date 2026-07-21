-- ============================================================
--  RUN_ALL.SQL
--  Tum kurulum betiklerini dogru sirada calistirir.
--  Nesneler, baglanan kullanicinin (ornn. ADMIN veya APPUSER)
--  kendi semasinda olusturulur; sabit bir semaya baglanmaz.
--  Calistirma: proje kok dizininden  ->  @DB/run_all.sql
-- ============================================================

-- 1. Temizlik (eskiyen/kullanilmayan nesneler)
@@05_Cleanup_Unused_Objects.sql

-- 2. Tablolari ve iliskili nesneleri olustur
@@01_Tables.sql

-- 3. Trigger ve package/prosedurleri derle
@@02_Triggers.sql
@@03_Procedures.sql

-- 4. Ornek calistirma adimlari
@@04_Examples.sql
