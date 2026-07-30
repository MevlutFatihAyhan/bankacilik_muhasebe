-- ============================================================
--  02_TRIGGERS.SQL
--  Tüm trigger tanımları
--  Çalıştırma sırası: 01_Tables.sql'den sonra çalıştırılmalıdır.
-- ============================================================

-- ============================================================
-- YARDIMCI FONKSIYON: aktif admin kullanici adi
-- ============================================================


CREATE OR REPLACE TRIGGER TRG_HESAP_BAKIYE_KONTROL
BEFORE UPDATE OF BAKIYE ON MVD_HESAP
FOR EACH ROW
BEGIN
    IF :NEW.BAKIYE < 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'Hesap bakiyesi sifirin altina dusemez. (Yetersiz Bakiye)');
    END IF;
END;
/


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
DECLARE
    v_musteri_tipi NUMBER(1);
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
    
    -- YENI KURAL: Tuzel musteriler (Ticari hesaplar) Vadeli hesap acamaz
    IF :NEW.HESAP_TURU = 'Vadeli' THEN
        SELECT MUSTERI_TIPI INTO v_musteri_tipi FROM MST_MUSTERI WHERE MUSTERI_ID = :NEW.MUSTERI_ID;
        IF v_musteri_tipi = 2 THEN
            RAISE_APPLICATION_ERROR(-20060, 'Tuzel musteriler Vadeli hesap acamaz!');
        END IF;
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

-- ============================================================
-- TRIGGER: TRG_MVD_HESAPHAREKET_SIL_BAKIYE
-- Tablo : MVD_HESAPHAREKET
-- Olay  : BEFORE DELETE
-- Görev : Bir hareket silindiginde MVD_HESAP bakiyesini geri al.
--          Ekleme (INSERT) trigger'i bakiyeyi degistirdigi icin, silme
--          isleminde de simetrik olarak bakiyenin duzeltilmesi gerekir.
--          B (para girisi) silinirse bakiyeden dusulur; C (para cikisi)
--          silinirse bakiyeye geri eklenir.
-- ============================================================

CREATE OR REPLACE TRIGGER TRG_MVD_HESAPHAREKET_SIL_BAKIYE
BEFORE DELETE ON MVD_HESAPHAREKET
FOR EACH ROW
BEGIN
    IF :OLD.ISLEM_YONU = 'B' THEN
        -- B = Para girisi silindi: bakiyeden geri al
        UPDATE MVD_HESAP
           SET BAKIYE = BAKIYE - :OLD.ISLEM_TUTARI
         WHERE HESAP_NO = :OLD.HESAP_NO;
    ELSE
        -- C = Para cikisi silindi: bakiyeye geri ekle
        UPDATE MVD_HESAP
           SET BAKIYE = BAKIYE + :OLD.ISLEM_TUTARI
         WHERE HESAP_NO = :OLD.HESAP_NO;
    END IF;
END TRG_MVD_HESAPHAREKET_SIL_BAKIYE;
/

-- ============================================================
-- TRIGGER: TRG_MVD_HESAPHAREKET_HACIM
-- Tablo : MVD_HESAPHAREKET
-- Olay  : AFTER INSERT OR UPDATE OR DELETE (FOR EACH ROW)
-- Görev : MVD_GUNLUK_ISLEM_HACMI tablosunu artımlı olarak güncelle.
-- Not   : Burada MERGE KULLANILMAZ. Autonomous Database MERGE'i paralel
--          calistirdiginda ayni gune ait satir icin ORA-12860 (sibling row
--          lock deadlock) aliniyordu; tek transaction icinde iki hareket
--          yazan para transferi bu yuzden basarisiz oluyordu. Yerine
--          NO_PARALLEL ipucu ile UPDATE, satir yoksa INSERT yapiliyor.
-- ============================================================

CREATE OR REPLACE TRIGGER TRG_MVD_HESAPHAREKET_HACIM
AFTER INSERT OR UPDATE OR DELETE ON MVD_HESAPHAREKET
FOR EACH ROW
DECLARE
    PROCEDURE HACIM_ARTIR(P_TARIH IN DATE, P_TUTAR IN NUMBER) IS
    BEGIN
        UPDATE /*+ NO_PARALLEL */ MVD_GUNLUK_ISLEM_HACMI
           SET ISLEM_ADEDI           = ISLEM_ADEDI + 1,
               TOPLAM_MUTLAK_TUTAR   = TOPLAM_MUTLAK_TUTAR + ABS(P_TUTAR),
               SON_GUNCELLEME_TARIHI = SYSTIMESTAMP
         WHERE HACIM_TARIHI = P_TARIH;

        IF SQL%ROWCOUNT = 0 THEN
            BEGIN
                INSERT /*+ NO_PARALLEL */ INTO MVD_GUNLUK_ISLEM_HACMI (
                    HACIM_TARIHI, ISLEM_ADEDI, TOPLAM_MUTLAK_TUTAR, SON_GUNCELLEME_TARIHI
                ) VALUES (
                    P_TARIH, 1, ABS(P_TUTAR), SYSTIMESTAMP
                );
            EXCEPTION
                WHEN DUP_VAL_ON_INDEX THEN
                    -- Ayni gunu baska bir oturum araya girip eklemis olabilir.
                    UPDATE /*+ NO_PARALLEL */ MVD_GUNLUK_ISLEM_HACMI
                       SET ISLEM_ADEDI           = ISLEM_ADEDI + 1,
                           TOPLAM_MUTLAK_TUTAR   = TOPLAM_MUTLAK_TUTAR + ABS(P_TUTAR),
                           SON_GUNCELLEME_TARIHI = SYSTIMESTAMP
                     WHERE HACIM_TARIHI = P_TARIH;
            END;
        END IF;
    END HACIM_ARTIR;

    PROCEDURE HACIM_AZALT(P_TARIH IN DATE, P_TUTAR IN NUMBER) IS
    BEGIN
        UPDATE /*+ NO_PARALLEL */ MVD_GUNLUK_ISLEM_HACMI
           SET ISLEM_ADEDI           = ISLEM_ADEDI - 1,
               TOPLAM_MUTLAK_TUTAR   = TOPLAM_MUTLAK_TUTAR - ABS(P_TUTAR),
               SON_GUNCELLEME_TARIHI = SYSTIMESTAMP
         WHERE HACIM_TARIHI = P_TARIH;
    END HACIM_AZALT;
BEGIN
    IF INSERTING THEN
        HACIM_ARTIR(TRUNC(CAST(:NEW.ISLEM_TARIHI AS DATE)), :NEW.ISLEM_TUTARI);

    ELSIF DELETING THEN
        HACIM_AZALT(TRUNC(CAST(:OLD.ISLEM_TARIHI AS DATE)), :OLD.ISLEM_TUTARI);

    ELSIF UPDATING THEN
        -- Sadece tutar veya tarih değiştiyse işlem yap
        IF :OLD.ISLEM_TUTARI != :NEW.ISLEM_TUTARI
           OR TRUNC(CAST(:OLD.ISLEM_TARIHI AS DATE)) != TRUNC(CAST(:NEW.ISLEM_TARIHI AS DATE)) THEN
            HACIM_AZALT(TRUNC(CAST(:OLD.ISLEM_TARIHI AS DATE)), :OLD.ISLEM_TUTARI);
            HACIM_ARTIR(TRUNC(CAST(:NEW.ISLEM_TARIHI AS DATE)), :NEW.ISLEM_TUTARI);
        END IF;
    END IF;
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
            DOVIZ_CINSI, YENI_BAKIYE, ACIKLAMA, ISLEM_KODU, REFERANS_NO
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'I', FN_AKTIF_ADMIN_KULLANICI_ADI(), :NEW.ISLEM_ID, :NEW.HESAP_NO, :NEW.ISLEM_YONU, :NEW.ISLEM_TUTARI,
            :NEW.DOVIZ_CINSI, :NEW.YENI_BAKIYE, :NEW.ACIKLAMA, :NEW.ISLEM_KODU, :NEW.REFERANS_NO
        );
    ELSIF UPDATING THEN
        INSERT INTO MVD_HESAPHAREKET_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, ISLEM_ID, HESAP_NO, ISLEM_YONU, ISLEM_TUTARI,
            DOVIZ_CINSI, YENI_BAKIYE, ACIKLAMA, ISLEM_KODU, REFERANS_NO
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'U', FN_AKTIF_ADMIN_KULLANICI_ADI(), :NEW.ISLEM_ID, :NEW.HESAP_NO, :NEW.ISLEM_YONU, :NEW.ISLEM_TUTARI,
            :NEW.DOVIZ_CINSI, :NEW.YENI_BAKIYE, :NEW.ACIKLAMA, :NEW.ISLEM_KODU, :NEW.REFERANS_NO
        );
    ELSIF DELETING THEN
        INSERT INTO MVD_HESAPHAREKET_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, ISLEM_ID, HESAP_NO, ISLEM_YONU, ISLEM_TUTARI,
            DOVIZ_CINSI, YENI_BAKIYE, ACIKLAMA, ISLEM_KODU, REFERANS_NO
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'D', FN_AKTIF_ADMIN_KULLANICI_ADI(), :OLD.ISLEM_ID, :OLD.HESAP_NO, :OLD.ISLEM_YONU, :OLD.ISLEM_TUTARI,
            :OLD.DOVIZ_CINSI, :OLD.YENI_BAKIYE, :OLD.ACIKLAMA, :OLD.ISLEM_KODU, :OLD.REFERANS_NO
        );
    END IF;
END TRG_AUD_MVD_HESAPHAREKET;
/

-- ============================================================
-- TRIGGER: TRG_MUH_FIS_BI
-- Tablo : MUH_FIS
-- Olay  : BEFORE INSERT OR UPDATE
-- Görev : Muhasebe tarihini gune yuvarla, islem zamanini otomatik ata
-- ============================================================

CREATE OR REPLACE TRIGGER TRG_MUH_FIS_BI
BEFORE INSERT OR UPDATE ON MUH_FIS
FOR EACH ROW
BEGIN
    IF INSERTING THEN
        :NEW.MUHASEBETARIHI := TRUNC(NVL(:NEW.MUHASEBETARIHI, SYSDATE));
        :NEW.ISLEM_ZAMANI   := NVL(:NEW.ISLEM_ZAMANI, SYSTIMESTAMP);
    ELSE
        :NEW.MUHASEBETARIHI := TRUNC(:NEW.MUHASEBETARIHI);
    END IF;

    IF :NEW.ACIKLAMA IS NOT NULL THEN
        :NEW.ACIKLAMA := TRIM(:NEW.ACIKLAMA);
    END IF;
END TRG_MUH_FIS_BI;
/

-- ============================================================
-- TRIGGER: TRG_AUD_MUH_FIS
-- Tablo : MUH_FIS
-- Olay  : AFTER INSERT OR UPDATE OR DELETE
-- Görev : MUH_FIS_H gecmis tablosunu doldur
-- ============================================================

CREATE OR REPLACE TRIGGER TRG_AUD_MUH_FIS
AFTER INSERT OR UPDATE OR DELETE ON MUH_FIS
FOR EACH ROW
BEGIN
    IF INSERTING THEN
        INSERT INTO MUH_FIS_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, FIS_ID, ACIKLAMA, MUHASEBETARIHI, ISLEM_ZAMANI
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'I', FN_AKTIF_ADMIN_KULLANICI_ADI(), :NEW.FIS_ID, :NEW.ACIKLAMA, :NEW.MUHASEBETARIHI, :NEW.ISLEM_ZAMANI
        );
    ELSIF UPDATING THEN
        INSERT INTO MUH_FIS_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, FIS_ID, ACIKLAMA, MUHASEBETARIHI, ISLEM_ZAMANI
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'U', FN_AKTIF_ADMIN_KULLANICI_ADI(), :NEW.FIS_ID, :NEW.ACIKLAMA, :NEW.MUHASEBETARIHI, :NEW.ISLEM_ZAMANI
        );
    ELSIF DELETING THEN
        INSERT INTO MUH_FIS_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, FIS_ID, ACIKLAMA, MUHASEBETARIHI, ISLEM_ZAMANI
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'D', FN_AKTIF_ADMIN_KULLANICI_ADI(), :OLD.FIS_ID, :OLD.ACIKLAMA, :OLD.MUHASEBETARIHI, :OLD.ISLEM_ZAMANI
        );
    END IF;
END TRG_AUD_MUH_FIS;
/

-- ============================================================
-- TRIGGER: TRG_AUD_MUH_FIS_HAREKET
-- Tablo : MUH_FIS_HAREKET
-- Olay  : AFTER INSERT OR UPDATE OR DELETE
-- Görev : MUH_FIS_HAREKET_H gecmis tablosunu doldur
-- ============================================================

CREATE OR REPLACE TRIGGER TRG_AUD_MUH_FIS_HAREKET
AFTER INSERT OR UPDATE OR DELETE ON MUH_FIS_HAREKET
FOR EACH ROW
BEGIN
    IF INSERTING THEN
        INSERT INTO MUH_FIS_HAREKET_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, FIS_HAREKET_ID, FIS_ID, HESAP_NO, BORC_TUTARI, ALACAK_TUTARI, SKONT, DOVIZ_CINSI
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'I', FN_AKTIF_ADMIN_KULLANICI_ADI(), :NEW.FIS_HAREKET_ID, :NEW.FIS_ID, :NEW.HESAP_NO, :NEW.BORC_TUTARI, :NEW.ALACAK_TUTARI, :NEW.SKONT, :NEW.DOVIZ_CINSI
        );
    ELSIF UPDATING THEN
        INSERT INTO MUH_FIS_HAREKET_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, FIS_HAREKET_ID, FIS_ID, HESAP_NO, BORC_TUTARI, ALACAK_TUTARI, SKONT, DOVIZ_CINSI
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'U', FN_AKTIF_ADMIN_KULLANICI_ADI(), :NEW.FIS_HAREKET_ID, :NEW.FIS_ID, :NEW.HESAP_NO, :NEW.BORC_TUTARI, :NEW.ALACAK_TUTARI, :NEW.SKONT, :NEW.DOVIZ_CINSI
        );
    ELSIF DELETING THEN
        INSERT INTO MUH_FIS_HAREKET_H (
            H_ID, H_ISLEM_TIPI, H_ISLEM_YAPAN, FIS_HAREKET_ID, FIS_ID, HESAP_NO, BORC_TUTARI, ALACAK_TUTARI, SKONT, DOVIZ_CINSI
        ) VALUES (
            SEQ_ISLEM_H_ID.NEXTVAL, 'D', FN_AKTIF_ADMIN_KULLANICI_ADI(), :OLD.FIS_HAREKET_ID, :OLD.FIS_ID, :OLD.HESAP_NO, :OLD.BORC_TUTARI, :OLD.ALACAK_TUTARI, :OLD.SKONT, :OLD.DOVIZ_CINSI
        );
    END IF;
END TRG_AUD_MUH_FIS_HAREKET;
/

-- ============================================================
-- TRIGGER: TRG_MVD_HESAPHAREKET_FIS_OLUSTUR
-- Tablo : MVD_HESAPHAREKET
-- Olay  : AFTER INSERT
-- Görev : MVD_HESAPHAREKET tablosuna yeni bir işlem eklendiğinde
--          otomatik olarak MUH_FIS ve MUH_FIS_HAREKET tablosuna 
--          muhasebe yevmiye fişi kaydı atar.
--          ISLEM_YONU kuralı: 
--            'B' -> Borç (Para Girişi)
--            'C' -> Alacak (Para Çıkışı)
-- ============================================================

CREATE OR REPLACE TRIGGER TRG_MVD_HESAPHAREKET_FIS_OLUSTUR
AFTER INSERT ON MVD_HESAPHAREKET
FOR EACH ROW
DECLARE
    v_fis_id       NUMBER;
    v_skont        VARCHAR2(50);
    v_borc_tutar   NUMBER(18, 4) := 0;
    v_alacak_tutar NUMBER(18, 4) := 0;
BEGIN
    -- 1. Muhasebe Fişi Başlığı Oluştur
    INSERT INTO MUH_FIS (
        ACIKLAMA,
        MUHASEBETARIHI,
        ISLEM_ZAMANI
    ) VALUES (
        NVL(:NEW.ACIKLAMA, 'Bankacılık İşlemi'),
        TRUNC(NVL(:NEW.ISLEM_TARIHI, SYSDATE)),
        SYSTIMESTAMP
    ) RETURNING FIS_ID INTO v_fis_id;

    -- 2. Tablodaki ISLEM_YONU Kuralına göre ('B' = Borç, 'C' = Alacak) Tutar Dağılımı
    IF UPPER(:NEW.ISLEM_YONU) = 'B' THEN
        v_borc_tutar   := :NEW.ISLEM_TUTARI;
        v_alacak_tutar := 0;
    ELSIF UPPER(:NEW.ISLEM_YONU) = 'C' THEN
        v_alacak_tutar := :NEW.ISLEM_TUTARI;
        v_borc_tutar   := 0;
    ELSE
        -- ISLEM_YONU boş ise ISLEM_KODU kontrolü yapılır
        IF UPPER(:NEW.ISLEM_KODU) = 'B' THEN
            v_borc_tutar   := :NEW.ISLEM_TUTARI;
            v_alacak_tutar := 0;
        ELSE
            v_alacak_tutar := :NEW.ISLEM_TUTARI;
            v_borc_tutar   := 0;
        END IF;
    END IF;

    v_skont :=  '002-Bankacılık İşlemleri' ;

    -- 3. Muhasebe Fişi Detay Satırını Oluştur (MUH_FIS_HAREKET)
    INSERT INTO MUH_FIS_HAREKET (
        FIS_ID,
        HESAP_NO,
        BORC_TUTARI,
        ALACAK_TUTARI,
        SKONT,
        DOVIZ_CINSI
    ) VALUES (
        v_fis_id,
        :NEW.HESAP_NO,
        v_borc_tutar,
        v_alacak_tutar,
        v_skont,
        NVL(:NEW.DOVIZ_CINSI, 'TRY')
    );
END TRG_MVD_HESAPHAREKET_FIS_OLUSTUR;
/    
    
    
    
