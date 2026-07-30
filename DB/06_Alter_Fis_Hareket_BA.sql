-- ============================================================
--  06_ALTER_FIS_HAREKET_BA.SQL
--  Mevcut kurulumlara MUH_FIS_HAREKET.BORC_ALACAK ('B'/'A') kolonunu
--  ekleyen bakim betigidir. 01_Tables.sql yeni kurulumlarda kolonu
--  zaten olusturur; bu betik idempotenttir (kolon varsa hicbir sey yapmaz).
--  Calistirma sirasi: 01_Tables.sql sonrasi, 02_Triggers.sql oncesi.
-- ============================================================

SET SERVEROUTPUT ON;

DECLARE
    v_var NUMBER;

    PROCEDURE ekle(p_tablo VARCHAR2, p_notnull BOOLEAN) IS
        v_sayi NUMBER;
    BEGIN
        SELECT COUNT(*)
          INTO v_sayi
          FROM USER_TAB_COLUMNS
         WHERE TABLE_NAME  = p_tablo
           AND COLUMN_NAME = 'BORC_ALACAK';

        IF v_sayi > 0 THEN
            DBMS_OUTPUT.PUT_LINE(p_tablo || '.BORC_ALACAK zaten mevcut. Islem yapilmadi.');
            RETURN;
        END IF;

        EXECUTE IMMEDIATE 'ALTER TABLE ' || p_tablo || ' ADD (BORC_ALACAK CHAR(1))';

        -- Mevcut satirlarin yonunu tutarlardan turet
        EXECUTE IMMEDIATE
            'UPDATE ' || p_tablo || '
                SET BORC_ALACAK = CASE WHEN NVL(BORC_TUTARI, 0) > 0 THEN ''B''
                                       WHEN NVL(ALACAK_TUTARI, 0) > 0 THEN ''A''
                                       ELSE ''B'' END';
        COMMIT;

        IF p_notnull THEN
            EXECUTE IMMEDIATE 'ALTER TABLE ' || p_tablo || ' MODIFY BORC_ALACAK NOT NULL';
            EXECUTE IMMEDIATE
                'ALTER TABLE ' || p_tablo || '
                     ADD CONSTRAINT CHK_FIS_HAREKET_BA CHECK (BORC_ALACAK IN (''B'', ''A''))';
            EXECUTE IMMEDIATE
                'ALTER TABLE ' || p_tablo || '
                     ADD CONSTRAINT CHK_FIS_HAREKET_BA_TUTAR
                     CHECK ((BORC_ALACAK = ''B'' AND ALACAK_TUTARI = 0)
                            OR (BORC_ALACAK = ''A'' AND BORC_TUTARI = 0))';
        END IF;

        DBMS_OUTPUT.PUT_LINE(p_tablo || '.BORC_ALACAK eklendi ve dolduruldu.');
    END ekle;
BEGIN
    SELECT COUNT(*) INTO v_var FROM USER_TABLES WHERE TABLE_NAME = 'MUH_FIS_HAREKET';
    IF v_var = 0 THEN
        DBMS_OUTPUT.PUT_LINE('MUH_FIS_HAREKET tablosu bulunamadi.');
        RETURN;
    END IF;

    ekle('MUH_FIS_HAREKET', TRUE);
    -- Gecmis tablosunda kisit/NOT NULL yok (eski kayitlar icin bos kalabilir)
    ekle('MUH_FIS_HAREKET_H', FALSE);
END;
/
