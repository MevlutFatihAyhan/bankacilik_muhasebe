import oracledb

DB_USER = "ADMIN"
DB_PASSWORD = "MFZNAyhan2005."
DB_DSN = "(description= (retry_count=20)(retry_delay=3)(address=(protocol=tcps)(port=1521)(host=adb.eu-frankfurt-1.oraclecloud.com))(connect_data=(service_name=gd33e9612e30a83_mfabank_medium.adb.oraclecloud.com))(security=(ssl_server_dn_match=yes)))"

trigger_sql = """
CREATE OR REPLACE TRIGGER TRG_MVD_HESAP_BI
BEFORE INSERT OR UPDATE ON MVD_HESAP
FOR EACH ROW
DECLARE
    v_musteri_tipi NUMBER(1);
BEGIN
    IF :NEW.IBAN IS NOT NULL THEN
        :NEW.IBAN := UPPER(TRIM(:NEW.IBAN));
    END IF;

    IF :NEW.HESAP_TURU IS NOT NULL THEN
        :NEW.HESAP_TURU := INITCAP(TRIM(:NEW.HESAP_TURU));
    END IF;

    :NEW.DOVIZ_CINSI := UPPER(TRIM(:NEW.DOVIZ_CINSI));
    :NEW.BAKIYE := NVL(:NEW.BAKIYE, 0);

    IF INSERTING THEN
        :NEW.DURUM := NVL(:NEW.DURUM, 1);
    END IF;

    IF :NEW.DURUM NOT IN (1, 2, 3) THEN
        RAISE_APPLICATION_ERROR(-20040, 'Hesap durumu 1 (Aktif), 2 (Pasif) veya 3 (Kapali) olmalidir.');
    END IF;
    
    IF :NEW.HESAP_TURU = 'Vadeli' THEN
        SELECT MUSTERI_TIPI INTO v_musteri_tipi FROM MST_MUSTERI WHERE MUSTERI_ID = :NEW.MUSTERI_ID;
        IF v_musteri_tipi = 2 THEN
            RAISE_APPLICATION_ERROR(-20060, 'Tuzel musteriler Vadeli hesap acamaz!');
        END IF;
    END IF;
END;
"""

try:
    connection = oracledb.connect(user=DB_USER, password=DB_PASSWORD, dsn=DB_DSN)
    cursor = connection.cursor()
    cursor.execute(trigger_sql)
    print("Trigger başarıyla güncellendi!")
    cursor.close()
    connection.close()
except Exception as e:
    print(f"Hata: {e}")
