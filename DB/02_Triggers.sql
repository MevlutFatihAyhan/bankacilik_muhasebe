-- ============================================================
--  02_TRIGGERS.SQL
--  Tüm trigger tanımları
--  Çalıştırma sırası: 01_Tables.sql'den sonra çalıştırılmalıdır.
-- ============================================================

-- ============================================================
-- YARDIMCI FONKSIYON: aktif admin kullanici adi
-- ============================================================

CREATE OR REPLACE FUNCTION FN_AKTIF_ADMIN_KULLANICI_ADI
    RETURN VARCHAR2
IS
    v_kullanici_adi VARCHAR2(50);
BEGIN
    SELECT KULLANICI_ADI
      INTO v_kullanici_adi
      FROM MVD_ADMIN
     WHERE UPPER(KULLANICI_ADI) = UPPER(SYS_CONTEXT('USERENV', 'SESSION_USER'))
       AND ROWNUM = 1;

    RETURN v_kullanici_adi;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        BEGIN
            SELECT KULLANICI_ADI
              INTO v_kullanici_adi
              FROM (
                    SELECT KULLANICI_ADI
                      FROM MVD_ADMIN
                     ORDER BY ADMIN_ID
                   )
             WHERE ROWNUM = 1;

            RETURN v_kullanici_adi;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                RETURN SYS_CONTEXT('USERENV', 'SESSION_USER');
        END;
END FN_AKTIF_ADMIN_KULLANICI_ADI;
/

-- ============================================================
-- TRIGGER: TRG_MST_MUSTERI_BI
-- Tablo : MST_MUSTERI
-- Olay  : BEFORE INSERT OR UPDATE
-- Görev : INSERT'te sequence ile ID atama + veri temizleme
--          UPDATE'te veri temizleme
--          Her iki durumda GUNCELLEME_TARIHI otomatik güncelleme
-- ============================================================

CREATE OR REPLACE TRIGGER TRG_MST_MUSTERI_BI
BEFORE INSERT OR UPDATE ON MST_MUSTERI
FOR EACH ROW
BEGIN
    IF INSERTING THEN
        :NEW.AD               := TRIM(:NEW.AD);
        :NEW.SOYAD            := TRIM(:NEW.SOYAD);
        :NEW.EMAIL            := LOWER(TRIM(:NEW.EMAIL));
        :NEW.TELEFON          := TRIM(:NEW.TELEFON);
        :NEW.AKTIF_MI         := NVL(:NEW.AKTIF_MI, 1);
        :NEW.OLUSTURMA_TARIHI := NVL(:NEW.OLUSTURMA_TARIHI, SYSDATE);
    ELSE
        -- UPDATE: Veri temizleme
        :NEW.AD      := TRIM(:NEW.AD);
        :NEW.SOYAD   := TRIM(:NEW.SOYAD);
        :NEW.EMAIL   := LOWER(TRIM(:NEW.EMAIL));
        :NEW.TELEFON := TRIM(:NEW.TELEFON);
    END IF;

    -- Her INSERT ve UPDATE'te güncelleme tarihi otomatik ayarlanır
    :NEW.GUNCELLEME_TARIHI := SYSDATE;
END TRG_MST_MUSTERI_BI;
/

-- ============================================================
-- TRIGGER: TRG_MST_MUSTERIADRES_BI
-- Tablo : MST_MUSTERIADRES
-- Olay  : BEFORE INSERT OR UPDATE
-- Görev : Adres alanlarını normalize et (INITCAP, TRIM)
--          Tarih alanlarını otomatik yönet
-- ============================================================

CREATE OR REPLACE TRIGGER TRG_MST_MUSTERIADRES_BI
BEFORE INSERT OR UPDATE ON MST_MUSTERIADRES
FOR EACH ROW
BEGIN
    IF INSERTING THEN
        :NEW.OLUSTURMA_TARIHI := NVL(:NEW.OLUSTURMA_TARIHI, SYSDATE);
    END IF;

    :NEW.GUNCELLEME_TARIHI := SYSDATE;
    :NEW.ULKE  := INITCAP(TRIM(:NEW.ULKE));
    :NEW.SEHIR := INITCAP(TRIM(:NEW.SEHIR));

    IF :NEW.ILCE IS NOT NULL THEN
        :NEW.ILCE := INITCAP(TRIM(:NEW.ILCE));
    END IF;

    IF :NEW.ADRES_BASLIK IS NOT NULL THEN
        :NEW.ADRES_BASLIK := TRIM(:NEW.ADRES_BASLIK);
    END IF;

    IF :NEW.POSTA_KODU IS NOT NULL THEN
        :NEW.POSTA_KODU := TRIM(:NEW.POSTA_KODU);
    END IF;
END TRG_MST_MUSTERIADRES_BI;
/

-- ============================================================
-- TRIGGER: TRG_MVD_HESAP_BI
-- Tablo : MVD_HESAP
-- Olay  : BEFORE INSERT OR UPDATE
-- Görev : IBAN ve DOVIZ_CINSI büyük harfe çevir
--          BAKIYE NULL gelirse 0 ata
--          DURUM geçerliliğini kontrol et (AKTIF/PASIF/KAPALI)
-- ============================================================

CREATE OR REPLACE TRIGGER TRG_MVD_HESAP_BI
BEFORE INSERT OR UPDATE ON MVD_HESAP
FOR EACH ROW
BEGIN
    IF :NEW.IBAN IS NOT NULL THEN
        :NEW.IBAN := UPPER(TRIM(:NEW.IBAN));
    END IF;

    IF :NEW.HESAP_TURU IS NOT NULL THEN
        -- PKG_DASHBOARD.PRC_GET_SUMMARY 'Vadesiz'/'Vadeli' seklinde (Baslik Harfli)
        -- karsilastirma yaptigi icin veri girisinde buyuk/kucuk harf farkindan
        -- kaynaklanan uyusmazliklari onlemek amaciyla burada normalize ediyoruz.
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
END TRG_MVD_HESAP_BI;
/

-- ============================================================
-- TRIGGER: TRG_MVD_HESAPHAREKET_BAKIYE
-- Tablo : MVD_HESAPHAREKET
-- Olay  : BEFORE INSERT
-- Görev : Bakiye hesaplama ve MVD_HESAP güncellemesi
--          Bu trigger sayesinde sp_hesap_hareket_ekle prosedürünün
--          ayrıca UPDATE MVD_HESAP yapmasına gerek yoktur.
-- Not   : ISLEM_YONU → B = Borc (Para Girisi), C = Cari (Para Cikisi)
-- ============================================================

CREATE OR REPLACE TRIGGER TRG_MVD_HESAPHAREKET_BAKIYE
BEFORE INSERT ON MVD_HESAPHAREKET
FOR EACH ROW
DECLARE
    v_mevcut_bakiye NUMBER(18, 4);
    v_hesap_doviz   VARCHAR2(3);
    v_hesap_durum   NUMBER(1);
BEGIN
    :NEW.DOVIZ_CINSI := UPPER(TRIM(:NEW.DOVIZ_CINSI));
    :NEW.ISLEM_YONU  := UPPER(TRIM(:NEW.ISLEM_YONU));

    -- Tutar ve yön doğrulaması
    IF :NEW.ISLEM_TUTARI <= 0 THEN
        RAISE_APPLICATION_ERROR(-20030, 'ISLEM_TUTARI sifirdan buyuk olmalidir.');
    END IF;

    IF :NEW.ISLEM_YONU NOT IN ('B', 'C') THEN
        RAISE_APPLICATION_ERROR(-20031, 'ISLEM_YONU B veya C olmalidir.');
    END IF;

    -- Hesabı kilitle ve mevcut bakiyeyi, dövizi ve durumu oku
    SELECT BAKIYE, DOVIZ_CINSI, DURUM
      INTO v_mevcut_bakiye, v_hesap_doviz, v_hesap_durum
      FROM MVD_HESAP
     WHERE HESAP_NO = :NEW.HESAP_NO
       FOR UPDATE;

    IF v_hesap_durum <> 1 THEN
        RAISE_APPLICATION_ERROR(-20034, 'Pasif veya kapali hesaplar uzerinde islem yapilamaz.');
    END IF;

    IF v_hesap_doviz <> :NEW.DOVIZ_CINSI THEN
        RAISE_APPLICATION_ERROR(-20033, 'Islem para birimi hesap para birimiyle eslesmelidir.');
    END IF;

    -- Yeni bakiyeyi hesapla
    IF :NEW.ISLEM_YONU = 'B' THEN
        -- B = Borc: Para girisi
        :NEW.YENI_BAKIYE := v_mevcut_bakiye + :NEW.ISLEM_TUTARI;
    ELSE
        -- C = Cari: Para cikisi
        IF v_mevcut_bakiye < :NEW.ISLEM_TUTARI THEN
            RAISE_APPLICATION_ERROR(-20032, 'Yetersiz bakiye.');
        END IF;
        :NEW.YENI_BAKIYE := v_mevcut_bakiye - :NEW.ISLEM_TUTARI;
    END IF;

    -- İşlem tarihini ata
    :NEW.ISLEM_TARIHI := NVL(:NEW.ISLEM_TARIHI, SYSTIMESTAMP);

    -- MVD_HESAP bakiyesini güncelle
    UPDATE MVD_HESAP
       SET BAKIYE = :NEW.YENI_BAKIYE
     WHERE HESAP_NO = :NEW.HESAP_NO;
END TRG_MVD_HESAPHAREKET_BAKIYE;
/

-- ============================================================
-- TRIGGER: TRG_MVD_HESAPHAREKET_PREVENT_UPDATE
-- Tablo : MVD_HESAPHAREKET
-- Olay  : BEFORE UPDATE
-- Görev : Hareket kayıtlarının güncellenmesini engelle
--          Düzeltme gerekiyorsa yeni bir ters işlem eklenmeli
-- ============================================================

CREATE OR REPLACE TRIGGER TRG_MVD_HESAPHAREKET_PREVENT_UPDATE
BEFORE UPDATE ON MVD_HESAPHAREKET
FOR EACH ROW
BEGIN
    RAISE_APPLICATION_ERROR(-20050,
        'Hesap hareketi guncellenemez. Duzeltme icin yeni bir islem ekleyin.');
END TRG_MVD_HESAPHAREKET_PREVENT_UPDATE;
/

CREATE OR REPLACE TRIGGER TRG_MVD_HESAPHAREKET_HACIM
AFTER INSERT OR UPDATE OR DELETE ON MVD_HESAPHAREKET
BEGIN
        DELETE FROM MVD_GUNLUK_ISLEM_HACMI;

        INSERT INTO MVD_GUNLUK_ISLEM_HACMI (
                HACIM_TARIHI,
                ISLEM_ADEDI,
                TOPLAM_MUTLAK_TUTAR,
                SON_GUNCELLEME_TARIHI
        )
        SELECT TRUNC(CAST(ISLEM_TARIHI AS DATE)) AS HACIM_TARIHI,
                     COUNT(*) AS ISLEM_ADEDI,
                     SUM(ABS(ISLEM_TUTARI)) AS TOPLAM_MUTLAK_TUTAR,
                     SYSTIMESTAMP AS SON_GUNCELLEME_TARIHI
            FROM MVD_HESAPHAREKET
         WHERE CAST(ISLEM_TARIHI AS DATE) >= TRUNC(SYSDATE, 'MM')
             AND CAST(ISLEM_TARIHI AS DATE) < TRUNC(SYSDATE) + 1
         GROUP BY TRUNC(CAST(ISLEM_TARIHI AS DATE));
END TRG_MVD_HESAPHAREKET_HACIM;
/

-- ============================================================
-- AUDIT TRIGGERS (HISTORY)
-- ============================================================

CREATE OR REPLACE TRIGGER TRG_AUD_MST_MUSTERI
AFTER INSERT OR UPDATE OR DELETE ON MST_MUSTERI
FOR EACH ROW
BEGIN
    IF INSERTING THEN
        INSERT INTO MST_MUSTERI_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, MUSTERI_ID, AD, SOYAD, MUSTERI_TIPI, KIMLIK_NO,
            EMAIL, TELEFON, AKTIF_MI
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'I', FN_AKTIF_ADMIN_KULLANICI_ADI(), :NEW.MUSTERI_ID, :NEW.AD, :NEW.SOYAD, :NEW.MUSTERI_TIPI, :NEW.KIMLIK_NO,
            :NEW.EMAIL, :NEW.TELEFON, :NEW.AKTIF_MI
        );
    ELSIF UPDATING THEN
        INSERT INTO MST_MUSTERI_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, MUSTERI_ID, AD, SOYAD, MUSTERI_TIPI, KIMLIK_NO,
            EMAIL, TELEFON, AKTIF_MI
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'U', FN_AKTIF_ADMIN_KULLANICI_ADI(), :NEW.MUSTERI_ID, :NEW.AD, :NEW.SOYAD, :NEW.MUSTERI_TIPI, :NEW.KIMLIK_NO,
            :NEW.EMAIL, :NEW.TELEFON, :NEW.AKTIF_MI
        );
    ELSIF DELETING THEN
        INSERT INTO MST_MUSTERI_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, MUSTERI_ID, AD, SOYAD, MUSTERI_TIPI, KIMLIK_NO,
            EMAIL, TELEFON, AKTIF_MI
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'D', FN_AKTIF_ADMIN_KULLANICI_ADI(), :OLD.MUSTERI_ID, :OLD.AD, :OLD.SOYAD, :OLD.MUSTERI_TIPI, :OLD.KIMLIK_NO,
            :OLD.EMAIL, :OLD.TELEFON, :OLD.AKTIF_MI
        );
    END IF;
END TRG_AUD_MST_MUSTERI;
/

CREATE OR REPLACE TRIGGER TRG_AUD_MST_MUSTERIADRES
AFTER INSERT OR UPDATE OR DELETE ON MST_MUSTERIADRES
FOR EACH ROW
BEGIN
    IF INSERTING THEN
        INSERT INTO MST_MUSTERIADRES_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, ADRES_ID, MUSTERI_ID, ADRES_BASLIK, ULKE, SEHIR, ILCE,
            POSTA_KODU, ACIK_ADRES
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'I', FN_AKTIF_ADMIN_KULLANICI_ADI(), :NEW.ADRES_ID, :NEW.MUSTERI_ID, :NEW.ADRES_BASLIK, :NEW.ULKE, :NEW.SEHIR, :NEW.ILCE,
            :NEW.POSTA_KODU, :NEW.ACIK_ADRES
        );
    ELSIF UPDATING THEN
        INSERT INTO MST_MUSTERIADRES_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, ADRES_ID, MUSTERI_ID, ADRES_BASLIK, ULKE, SEHIR, ILCE,
            POSTA_KODU, ACIK_ADRES
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'U', FN_AKTIF_ADMIN_KULLANICI_ADI(), :NEW.ADRES_ID, :NEW.MUSTERI_ID, :NEW.ADRES_BASLIK, :NEW.ULKE, :NEW.SEHIR, :NEW.ILCE,
            :NEW.POSTA_KODU, :NEW.ACIK_ADRES
        );
    ELSIF DELETING THEN
        INSERT INTO MST_MUSTERIADRES_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, ADRES_ID, MUSTERI_ID, ADRES_BASLIK, ULKE, SEHIR, ILCE,
            POSTA_KODU, ACIK_ADRES
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'D', FN_AKTIF_ADMIN_KULLANICI_ADI(), :OLD.ADRES_ID, :OLD.MUSTERI_ID, :OLD.ADRES_BASLIK, :OLD.ULKE, :OLD.SEHIR, :OLD.ILCE,
            :OLD.POSTA_KODU, :OLD.ACIK_ADRES
        );
    END IF;
END TRG_AUD_MST_MUSTERIADRES;
/

CREATE OR REPLACE TRIGGER TRG_AUD_MVD_HESAP
AFTER INSERT OR UPDATE OR DELETE ON MVD_HESAP
FOR EACH ROW
BEGIN
    IF INSERTING THEN
        INSERT INTO MVD_HESAP_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, HESAP_NO, MUSTERI_ID, IBAN, HESAP_TURU, DOVIZ_CINSI, BAKIYE, DURUM
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'I', FN_AKTIF_ADMIN_KULLANICI_ADI(), :NEW.HESAP_NO, :NEW.MUSTERI_ID, :NEW.IBAN, :NEW.HESAP_TURU, :NEW.DOVIZ_CINSI, :NEW.BAKIYE, :NEW.DURUM
        );
    ELSIF UPDATING THEN
        INSERT INTO MVD_HESAP_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, HESAP_NO, MUSTERI_ID, IBAN, HESAP_TURU, DOVIZ_CINSI, BAKIYE, DURUM
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'U', FN_AKTIF_ADMIN_KULLANICI_ADI(), :NEW.HESAP_NO, :NEW.MUSTERI_ID, :NEW.IBAN, :NEW.HESAP_TURU, :NEW.DOVIZ_CINSI, :NEW.BAKIYE, :NEW.DURUM
        );
    ELSIF DELETING THEN
        INSERT INTO MVD_HESAP_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, HESAP_NO, MUSTERI_ID, IBAN, HESAP_TURU, DOVIZ_CINSI, BAKIYE, DURUM
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'D', FN_AKTIF_ADMIN_KULLANICI_ADI(), :OLD.HESAP_NO, :OLD.MUSTERI_ID, :OLD.IBAN, :OLD.HESAP_TURU, :OLD.DOVIZ_CINSI, :OLD.BAKIYE, :OLD.DURUM
        );
    END IF;
END TRG_AUD_MVD_HESAP;
/

CREATE OR REPLACE TRIGGER TRG_AUD_MVD_HESAPHAREKET
AFTER INSERT OR UPDATE OR DELETE ON MVD_HESAPHAREKET
FOR EACH ROW
BEGIN
    IF INSERTING THEN
        INSERT INTO MVD_HESAPHAREKET_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, ISLEM_ID, HESAP_NO, ISLEM_YONU, ISLEM_TUTARI,
            DOVIZ_CINSI, YENI_BAKIYE, ISLEM_TARIHI, ACIKLAMA, ISLEM_KODU, REFERANS_NO
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'I', FN_AKTIF_ADMIN_KULLANICI_ADI(), :NEW.ISLEM_ID, :NEW.HESAP_NO, :NEW.ISLEM_YONU, :NEW.ISLEM_TUTARI,
            :NEW.DOVIZ_CINSI, :NEW.YENI_BAKIYE, :NEW.ISLEM_TARIHI, :NEW.ACIKLAMA, :NEW.ISLEM_KODU, :NEW.REFERANS_NO
        );
    ELSIF UPDATING THEN
        INSERT INTO MVD_HESAPHAREKET_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, ISLEM_ID, HESAP_NO, ISLEM_YONU, ISLEM_TUTARI,
            DOVIZ_CINSI, YENI_BAKIYE, ISLEM_TARIHI, ACIKLAMA, ISLEM_KODU, REFERANS_NO
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'U', FN_AKTIF_ADMIN_KULLANICI_ADI(), :NEW.ISLEM_ID, :NEW.HESAP_NO, :NEW.ISLEM_YONU, :NEW.ISLEM_TUTARI,
            :NEW.DOVIZ_CINSI, :NEW.YENI_BAKIYE, :NEW.ISLEM_TARIHI, :NEW.ACIKLAMA, :NEW.ISLEM_KODU, :NEW.REFERANS_NO
        );
    ELSIF DELETING THEN
        INSERT INTO MVD_HESAPHAREKET_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, ISLEM_ID, HESAP_NO, ISLEM_YONU, ISLEM_TUTARI,
            DOVIZ_CINSI, YENI_BAKIYE, ISLEM_TARIHI, ACIKLAMA, ISLEM_KODU, REFERANS_NO
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'D', FN_AKTIF_ADMIN_KULLANICI_ADI(), :OLD.ISLEM_ID, :OLD.HESAP_NO, :OLD.ISLEM_YONU, :OLD.ISLEM_TUTARI,
            :OLD.DOVIZ_CINSI, :OLD.YENI_BAKIYE, :OLD.ISLEM_TARIHI, :OLD.ACIKLAMA, :OLD.ISLEM_KODU, :OLD.REFERANS_NO
        );
    END IF;
END TRG_AUD_MVD_HESAPHAREKET;
/