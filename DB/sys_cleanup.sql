-- ============================================================
--  00_FULL_RESET.SQL
--  01_Tables.sql / 02_Triggers.sql / 03_Procedures.sql tarafindan
--  olusturulan TUM canli nesneleri siler, boylece 01_Tables.sql
--  bastan (temiz bir semada) tekrar calistirilabilir.
--
--  NOT: 05_Cleanup_Unused_Objects.sql BUNUN YERINE GECMEZ — o script
--  sadece eskiyen/kullanilmayan (legacy) nesneleri temizler, asil
--  tablolara dokunmaz. sys_cleanup.sql de sadece SYS semasindaki
--  kazara olusmus nesneleri hedefler.
--
--  DIKKAT: Bu script TUM VERIYI KALICI OLARAK SILER. Once yedek alin!
-- ============================================================

ALTER SESSION SET CURRENT_SCHEMA = APPUSER;

SET SERVEROUTPUT ON;

BEGIN
    -- 1) Tablolar (CASCADE CONSTRAINTS sayesinde FK sirasi onemli degil,
    --    ayrica tabloya bagli trigger'lar da otomatik silinir)
    FOR TBL IN (
        SELECT 'MVD_HESAPHAREKET_H'      AS TBL_NAME FROM DUAL
        UNION ALL SELECT 'MVD_HESAPHAREKET'    FROM DUAL
        UNION ALL SELECT 'MVD_HESAP_H'         FROM DUAL
        UNION ALL SELECT 'MVD_HESAP'           FROM DUAL
        UNION ALL SELECT 'MST_MUSTERIADRES_H'  FROM DUAL
        UNION ALL SELECT 'MST_MUSTERIADRES'    FROM DUAL
        UNION ALL SELECT 'MST_MUSTERI_H'       FROM DUAL
        UNION ALL SELECT 'MST_MUSTERI'         FROM DUAL
        UNION ALL SELECT 'MVD_ADMIN'           FROM DUAL
        UNION ALL SELECT 'MVD_GUNLUK_ISLEM_HACMI' FROM DUAL
    ) LOOP
        BEGIN
            EXECUTE IMMEDIATE 'DROP TABLE ' || TBL.TBL_NAME || ' CASCADE CONSTRAINTS PURGE';
            DBMS_OUTPUT.PUT_LINE(TBL.TBL_NAME || ' silindi.');
        EXCEPTION
            WHEN OTHERS THEN
                IF SQLCODE != -942 THEN -- -942: Table or view does not exist
                    RAISE;
                ELSE
                    DBMS_OUTPUT.PUT_LINE(TBL.TBL_NAME || ' zaten yok.');
                END IF;
        END;
    END LOOP;

    -- 2) Paketler
    FOR PKG IN (
        SELECT 'PKG_MUSTERI'   AS PKG_NAME FROM DUAL
        UNION ALL SELECT 'PKG_HESAP'     FROM DUAL
        UNION ALL SELECT 'PKG_DASHBOARD' FROM DUAL
    ) LOOP
        BEGIN
            EXECUTE IMMEDIATE 'DROP PACKAGE ' || PKG.PKG_NAME;
            DBMS_OUTPUT.PUT_LINE(PKG.PKG_NAME || ' silindi.');
        EXCEPTION
            WHEN OTHERS THEN
                IF SQLCODE != -4043 THEN -- -4043: Object does not exist
                    RAISE;
                ELSE
                    DBMS_OUTPUT.PUT_LINE(PKG.PKG_NAME || ' zaten yok.');
                END IF;
        END;
    END LOOP;

    -- 3) Standalone fonksiyon
    BEGIN
        EXECUTE IMMEDIATE 'DROP FUNCTION FN_AKTIF_ADMIN_KULLANICI_ADI';
        DBMS_OUTPUT.PUT_LINE('FN_AKTIF_ADMIN_KULLANICI_ADI silindi.');
    EXCEPTION
        WHEN OTHERS THEN
            IF SQLCODE != -4043 THEN
                RAISE;
            ELSE
                DBMS_OUTPUT.PUT_LINE('FN_AKTIF_ADMIN_KULLANICI_ADI zaten yok.');
            END IF;
    END;

    -- 4) Sequence
    BEGIN
        EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_ISLEM_H_ID';
        DBMS_OUTPUT.PUT_LINE('SEQ_ISLEM_H_ID silindi.');
    EXCEPTION
        WHEN OTHERS THEN
            IF SQLCODE != -2289 THEN -- -2289: Sequence does not exist
                RAISE;
            ELSE
                DBMS_OUTPUT.PUT_LINE('SEQ_ISLEM_H_ID zaten yok.');
            END IF;
    END;

    DBMS_OUTPUT.PUT_LINE(' Reset tamamlandi. ');
END;
/