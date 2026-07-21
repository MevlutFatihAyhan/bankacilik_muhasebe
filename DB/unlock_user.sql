-- ============================================================
--  UNLOCK_USER.SQL
--  SADECE yerel Oracle XE kurulumu icindir (APPUSER semasi).
--  Oracle Cloud (ADMIN) uzerinde bu betige GEREK YOKTUR.
--
--  Kullanim (yerel XE): once SYSDBA olarak baglanin, sonra bu betigi
--  calistirin:
--      sqlplus / as sysdba
--      @DB/unlock_user.sql
--
--  Not: Asagidaki komut ancak APPUSER kullanicisi mevcutsa calisir.
-- ============================================================

ALTER USER APPUSER IDENTIFIED BY AppUser123 ACCOUNT UNLOCK;
