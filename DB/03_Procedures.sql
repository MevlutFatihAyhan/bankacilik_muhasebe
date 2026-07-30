-- ============================================================
--  03_PROCEDURES.SQL
--  Package tanımları
--  Çalıştırma sırası: 02_Triggers.sql'den sonra çalıştırılmalıdır.
-- ============================================================

-- ============================================================
-- PACKAGE SPECIFICATION: PKG_MUSTERI
-- MST_MUSTERI ve MST_MUSTERIADRES tabloları için CRUD arayüzü
-- ============================================================

CREATE OR REPLACE PACKAGE PKG_MUSTERI AS

    -- --------------------------------------------------------
    -- MST_MUSTERI
    -- --------------------------------------------------------
    PROCEDURE PRC_MUSTERI_EKLE(
        P_AD         IN  MST_MUSTERI.AD%TYPE,
        P_SOYAD      IN  MST_MUSTERI.SOYAD%TYPE,
        P_MUSTERI_TIPI IN MST_MUSTERI.MUSTERI_TIPI%TYPE,
        P_KIMLIK_NO  IN  MST_MUSTERI.KIMLIK_NO%TYPE,
        P_EMAIL      IN  MST_MUSTERI.EMAIL%TYPE,
        P_TELEFON    IN  MST_MUSTERI.TELEFON%TYPE,
        P_AKTIF_MI   IN  MST_MUSTERI.AKTIF_MI%TYPE DEFAULT 1,
        P_MUSTERI_ID OUT MST_MUSTERI.MUSTERI_ID%TYPE
    );

    PROCEDURE PRC_MUSTERI_GUNCELLE(
        P_MUSTERI_ID IN MST_MUSTERI.MUSTERI_ID%TYPE,
        P_AD         IN MST_MUSTERI.AD%TYPE,
        P_SOYAD      IN MST_MUSTERI.SOYAD%TYPE,
        P_MUSTERI_TIPI IN MST_MUSTERI.MUSTERI_TIPI%TYPE,
        P_KIMLIK_NO  IN MST_MUSTERI.KIMLIK_NO%TYPE,
        P_EMAIL      IN MST_MUSTERI.EMAIL%TYPE,
        P_TELEFON    IN MST_MUSTERI.TELEFON%TYPE,
        P_AKTIF_MI   IN MST_MUSTERI.AKTIF_MI%TYPE
    );

    PROCEDURE PRC_MUSTERI_SIL(
        P_MUSTERI_ID IN MST_MUSTERI.MUSTERI_ID%TYPE
    );

    PROCEDURE PRC_MUSTERI_GETIR(
        P_MUSTERI_ID IN  MST_MUSTERI.MUSTERI_ID%TYPE,
        P_RESULT     OUT SYS_REFCURSOR
    );

    PROCEDURE PRC_MUSTERI_LISTE(
        P_SEARCH_TERM  IN  VARCHAR2 DEFAULT NULL,
        P_MUSTERI_TIPI IN  NUMBER DEFAULT NULL,
        P_AKTIF_MI     IN  NUMBER DEFAULT NULL,
        P_AD           IN  VARCHAR2 DEFAULT NULL,
        P_SOYAD        IN  VARCHAR2 DEFAULT NULL,
        P_RESULT       OUT SYS_REFCURSOR
    );

    PROCEDURE PRC_MUSTERI_OZET_LISTE(
        P_RESULT OUT SYS_REFCURSOR
    );

    -- --------------------------------------------------------
    -- MST_MUSTERIADRES
    -- --------------------------------------------------------
    PROCEDURE PRC_ADRES_EKLE(
        P_MUSTERI_ID   IN  MST_MUSTERIADRES.MUSTERI_ID%TYPE,
        P_ADRES_BASLIK IN  MST_MUSTERIADRES.ADRES_BASLIK%TYPE,
        P_ULKE         IN  MST_MUSTERIADRES.ULKE%TYPE,
        P_SEHIR        IN  MST_MUSTERIADRES.SEHIR%TYPE,
        P_ILCE         IN  MST_MUSTERIADRES.ILCE%TYPE,
        P_POSTA_KODU   IN  MST_MUSTERIADRES.POSTA_KODU%TYPE,
        P_ACIK_ADRES   IN  MST_MUSTERIADRES.ACIK_ADRES%TYPE,
        P_ADRES_ID     OUT MST_MUSTERIADRES.ADRES_ID%TYPE
    );

    PROCEDURE PRC_ADRES_GUNCELLE(
        P_ADRES_ID     IN MST_MUSTERIADRES.ADRES_ID%TYPE,
        P_ADRES_BASLIK IN MST_MUSTERIADRES.ADRES_BASLIK%TYPE,
        P_ULKE         IN MST_MUSTERIADRES.ULKE%TYPE,
        P_SEHIR        IN MST_MUSTERIADRES.SEHIR%TYPE,
        P_ILCE         IN MST_MUSTERIADRES.ILCE%TYPE,
        P_POSTA_KODU   IN MST_MUSTERIADRES.POSTA_KODU%TYPE,
        P_ACIK_ADRES   IN MST_MUSTERIADRES.ACIK_ADRES%TYPE
    );

    PROCEDURE PRC_ADRES_SIL(
        P_ADRES_ID IN MST_MUSTERIADRES.ADRES_ID%TYPE
    );

    PROCEDURE PRC_ADRES_GETIR(
        P_ADRES_ID IN  MST_MUSTERIADRES.ADRES_ID%TYPE,
        P_RESULT   OUT SYS_REFCURSOR
    );

    PROCEDURE PRC_MUSTERI_ADRESLERI(
        P_MUSTERI_ID IN  MST_MUSTERIADRES.MUSTERI_ID%TYPE,
        P_RESULT     OUT SYS_REFCURSOR
    );

END PKG_MUSTERI;
/

-- ============================================================
-- PACKAGE BODY: PKG_MUSTERI
-- ============================================================

CREATE OR REPLACE PACKAGE BODY PKG_MUSTERI AS

    -- Liste prosedurlerinin dondurecegi azami satir sayisi. Buyuk veri
    -- senaryosunda arayuze tum tabloyu tasimamak icin kullanilir.
    C_MAX_SATIR CONSTANT NUMBER := 10000;

    -- --------------------------------------------------------
    -- MST_MUSTERI — INSERT
    -- --------------------------------------------------------
    PROCEDURE PRC_MUSTERI_EKLE(
        P_AD         IN  MST_MUSTERI.AD%TYPE,
        P_SOYAD      IN  MST_MUSTERI.SOYAD%TYPE,
        P_MUSTERI_TIPI IN MST_MUSTERI.MUSTERI_TIPI%TYPE,
        P_KIMLIK_NO  IN  MST_MUSTERI.KIMLIK_NO%TYPE,
        P_EMAIL      IN  MST_MUSTERI.EMAIL%TYPE,
        P_TELEFON    IN  MST_MUSTERI.TELEFON%TYPE,
        P_AKTIF_MI   IN  MST_MUSTERI.AKTIF_MI%TYPE DEFAULT 1,
        P_MUSTERI_ID OUT MST_MUSTERI.MUSTERI_ID%TYPE
    ) IS
        V_COUNT NUMBER;
    BEGIN
        SELECT COUNT(1) INTO V_COUNT
          FROM MST_MUSTERI
         WHERE UPPER(EMAIL) = UPPER(P_EMAIL)
            OR KIMLIK_NO = P_KIMLIK_NO;

        IF V_COUNT > 0 THEN
            RAISE_APPLICATION_ERROR(-20001, 'Bu email veya kimlik no ile kayitli musteri zaten var.');
        END IF;

        INSERT INTO MST_MUSTERI (
            AD, SOYAD, MUSTERI_TIPI, KIMLIK_NO, EMAIL, TELEFON, AKTIF_MI, OLUSTURMA_TARIHI
        ) VALUES (
            P_AD, P_SOYAD, P_MUSTERI_TIPI, P_KIMLIK_NO, P_EMAIL, P_TELEFON, NVL(P_AKTIF_MI, 1), SYSDATE
        ) RETURNING MUSTERI_ID INTO P_MUSTERI_ID;
    END PRC_MUSTERI_EKLE;

    -- --------------------------------------------------------
    -- MST_MUSTERI — UPDATE
    -- --------------------------------------------------------
    PROCEDURE PRC_MUSTERI_GUNCELLE(
        P_MUSTERI_ID IN MST_MUSTERI.MUSTERI_ID%TYPE,
        P_AD         IN MST_MUSTERI.AD%TYPE,
        P_SOYAD      IN MST_MUSTERI.SOYAD%TYPE,
        P_MUSTERI_TIPI IN MST_MUSTERI.MUSTERI_TIPI%TYPE,
        P_KIMLIK_NO  IN MST_MUSTERI.KIMLIK_NO%TYPE,
        P_EMAIL      IN MST_MUSTERI.EMAIL%TYPE,
        P_TELEFON    IN MST_MUSTERI.TELEFON%TYPE,
        P_AKTIF_MI   IN MST_MUSTERI.AKTIF_MI%TYPE
    ) IS
        V_COUNT NUMBER;
    BEGIN
        SELECT COUNT(1) INTO V_COUNT
          FROM MST_MUSTERI
         WHERE (UPPER(EMAIL) = UPPER(P_EMAIL) OR KIMLIK_NO = P_KIMLIK_NO)
           AND MUSTERI_ID  <> P_MUSTERI_ID;

        IF V_COUNT > 0 THEN
            RAISE_APPLICATION_ERROR(-20002, 'Bu email veya kimlik no baska bir musteride kullaniliyor.');
        END IF;

        UPDATE MST_MUSTERI
           SET AD                = P_AD,
               SOYAD             = P_SOYAD,
               MUSTERI_TIPI      = P_MUSTERI_TIPI,
               KIMLIK_NO         = P_KIMLIK_NO,
               EMAIL             = P_EMAIL,
               TELEFON           = P_TELEFON,
               AKTIF_MI          = P_AKTIF_MI,
               GUNCELLEME_TARIHI = SYSDATE
         WHERE MUSTERI_ID = P_MUSTERI_ID;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20003, 'Musteri bulunamadi.');
        END IF;
    END PRC_MUSTERI_GUNCELLE;

    -- --------------------------------------------------------
    -- MST_MUSTERI — DELETE
    -- --------------------------------------------------------
    PROCEDURE PRC_MUSTERI_SIL(
        P_MUSTERI_ID IN MST_MUSTERI.MUSTERI_ID%TYPE
    ) IS
    BEGIN
        DELETE FROM MST_MUSTERI
         WHERE MUSTERI_ID = P_MUSTERI_ID;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20004, 'Silinecek musteri bulunamadi.');
        END IF;
    END PRC_MUSTERI_SIL;

    -- --------------------------------------------------------
    -- MST_MUSTERI — SELECT (Tek kayıt)
    -- --------------------------------------------------------
    PROCEDURE PRC_MUSTERI_GETIR(
        P_MUSTERI_ID IN  MST_MUSTERI.MUSTERI_ID%TYPE,
        P_RESULT     OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN P_RESULT FOR
        SELECT MUSTERI_ID, AD, SOYAD, MUSTERI_TIPI, KIMLIK_NO, EMAIL, TELEFON, AKTIF_MI,
               OLUSTURMA_TARIHI, GUNCELLEME_TARIHI
          FROM MST_MUSTERI
         WHERE MUSTERI_ID = P_MUSTERI_ID;
    END PRC_MUSTERI_GETIR;

    -- --------------------------------------------------------
    -- MST_MUSTERI — SELECT (Tüm liste)
    -- --------------------------------------------------------
    PROCEDURE PRC_MUSTERI_LISTE(
        P_SEARCH_TERM  IN  VARCHAR2 DEFAULT NULL,
        P_MUSTERI_TIPI IN  NUMBER DEFAULT NULL,
        P_AKTIF_MI     IN  NUMBER DEFAULT NULL,
        P_AD           IN  VARCHAR2 DEFAULT NULL,
        P_SOYAD        IN  VARCHAR2 DEFAULT NULL,
        P_RESULT       OUT SYS_REFCURSOR
    ) IS
    BEGIN
        -- Filtreleme tamamen DB tarafinda yapilir; P_AD tuzel musteride unvani karsilar.
        OPEN P_RESULT FOR
        SELECT MUSTERI_ID, AD, SOYAD, MUSTERI_TIPI, KIMLIK_NO, EMAIL, TELEFON, AKTIF_MI,
               OLUSTURMA_TARIHI, GUNCELLEME_TARIHI
          FROM MST_MUSTERI
         WHERE (P_SEARCH_TERM IS NULL OR (
                   UPPER(AD) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   UPPER(SOYAD) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   UPPER(KIMLIK_NO) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   UPPER(EMAIL) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   UPPER(TELEFON) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   TO_CHAR(MUSTERI_ID) LIKE '%' || P_SEARCH_TERM || '%'
               ))
           AND (P_MUSTERI_TIPI IS NULL OR MUSTERI_TIPI = P_MUSTERI_TIPI)
           AND (P_AKTIF_MI IS NULL OR AKTIF_MI = P_AKTIF_MI)
           AND (P_AD IS NULL OR UPPER(AD) LIKE '%' || UPPER(P_AD) || '%')
           AND (P_SOYAD IS NULL OR UPPER(SOYAD) LIKE '%' || UPPER(P_SOYAD) || '%')
         ORDER BY MUSTERI_ID DESC
         FETCH FIRST C_MAX_SATIR ROWS ONLY;
    END PRC_MUSTERI_LISTE;

    -- --------------------------------------------------------
    -- MST_MUSTERI — SELECT (Özet Liste)
    -- --------------------------------------------------------
    PROCEDURE PRC_MUSTERI_OZET_LISTE(
        P_RESULT OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN P_RESULT FOR
        SELECT MUSTERI_ID, AD, SOYAD
          FROM MST_MUSTERI
         ORDER BY MUSTERI_ID DESC;
    END PRC_MUSTERI_OZET_LISTE;

    -- --------------------------------------------------------
    -- MST_MUSTERIADRES — INSERT
    -- --------------------------------------------------------
    PROCEDURE PRC_ADRES_EKLE(
        P_MUSTERI_ID   IN  MST_MUSTERIADRES.MUSTERI_ID%TYPE,
        P_ADRES_BASLIK IN  MST_MUSTERIADRES.ADRES_BASLIK%TYPE,
        P_ULKE         IN  MST_MUSTERIADRES.ULKE%TYPE,
        P_SEHIR        IN  MST_MUSTERIADRES.SEHIR%TYPE,
        P_ILCE         IN  MST_MUSTERIADRES.ILCE%TYPE,
        P_POSTA_KODU   IN  MST_MUSTERIADRES.POSTA_KODU%TYPE,
        P_ACIK_ADRES   IN  MST_MUSTERIADRES.ACIK_ADRES%TYPE,
        P_ADRES_ID     OUT MST_MUSTERIADRES.ADRES_ID%TYPE
    ) IS
        V_COUNT NUMBER;
    BEGIN
        SELECT COUNT(1) INTO V_COUNT
          FROM MST_MUSTERI
         WHERE MUSTERI_ID = P_MUSTERI_ID;

        IF V_COUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20005, 'Adres eklenecek musteri bulunamadi.');
        END IF;

        INSERT INTO MST_MUSTERIADRES (
            MUSTERI_ID, ADRES_BASLIK, ULKE, SEHIR, ILCE,
            POSTA_KODU, ACIK_ADRES, OLUSTURMA_TARIHI
        ) VALUES (
            P_MUSTERI_ID, P_ADRES_BASLIK, P_ULKE, P_SEHIR, P_ILCE,
            P_POSTA_KODU, P_ACIK_ADRES, SYSDATE
        ) RETURNING ADRES_ID INTO P_ADRES_ID;
    END PRC_ADRES_EKLE;

    -- --------------------------------------------------------
    -- MST_MUSTERIADRES — UPDATE
    -- --------------------------------------------------------
    PROCEDURE PRC_ADRES_GUNCELLE(
        P_ADRES_ID     IN MST_MUSTERIADRES.ADRES_ID%TYPE,
        P_ADRES_BASLIK IN MST_MUSTERIADRES.ADRES_BASLIK%TYPE,
        P_ULKE         IN MST_MUSTERIADRES.ULKE%TYPE,
        P_SEHIR        IN MST_MUSTERIADRES.SEHIR%TYPE,
        P_ILCE         IN MST_MUSTERIADRES.ILCE%TYPE,
        P_POSTA_KODU   IN MST_MUSTERIADRES.POSTA_KODU%TYPE,
        P_ACIK_ADRES   IN MST_MUSTERIADRES.ACIK_ADRES%TYPE
    ) IS
    BEGIN
        UPDATE MST_MUSTERIADRES
           SET ADRES_BASLIK      = P_ADRES_BASLIK,
               ULKE              = P_ULKE,
               SEHIR             = P_SEHIR,
               ILCE              = P_ILCE,
               POSTA_KODU        = P_POSTA_KODU,
               ACIK_ADRES        = P_ACIK_ADRES,
               GUNCELLEME_TARIHI = SYSDATE
         WHERE ADRES_ID = P_ADRES_ID;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20006, 'Guncellenecek adres bulunamadi.');
        END IF;
    END PRC_ADRES_GUNCELLE;

    -- --------------------------------------------------------
    -- MST_MUSTERIADRES — DELETE
    -- --------------------------------------------------------
    PROCEDURE PRC_ADRES_SIL(
        P_ADRES_ID IN MST_MUSTERIADRES.ADRES_ID%TYPE
    ) IS
    BEGIN
        DELETE FROM MST_MUSTERIADRES
         WHERE ADRES_ID = P_ADRES_ID;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20007, 'Silinecek adres bulunamadi.');
        END IF;
    END PRC_ADRES_SIL;

    -- --------------------------------------------------------
    -- MST_MUSTERIADRES — SELECT (Tek kayıt)
    -- --------------------------------------------------------
    PROCEDURE PRC_ADRES_GETIR(
        P_ADRES_ID IN  MST_MUSTERIADRES.ADRES_ID%TYPE,
        P_RESULT   OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN P_RESULT FOR
        SELECT ADRES_ID, MUSTERI_ID, ADRES_BASLIK, ULKE, SEHIR, ILCE,
               POSTA_KODU, ACIK_ADRES, OLUSTURMA_TARIHI, GUNCELLEME_TARIHI
          FROM MST_MUSTERIADRES
         WHERE ADRES_ID = P_ADRES_ID;
    END PRC_ADRES_GETIR;

    -- --------------------------------------------------------
    -- MST_MUSTERIADRES — SELECT (Müşterinin tüm adresleri)
    -- --------------------------------------------------------
    PROCEDURE PRC_MUSTERI_ADRESLERI(
        P_MUSTERI_ID IN  MST_MUSTERIADRES.MUSTERI_ID%TYPE,
        P_RESULT     OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN P_RESULT FOR
        SELECT ADRES_ID, MUSTERI_ID, ADRES_BASLIK, ULKE, SEHIR, ILCE,
               POSTA_KODU, ACIK_ADRES, OLUSTURMA_TARIHI, GUNCELLEME_TARIHI
          FROM MST_MUSTERIADRES
         WHERE MUSTERI_ID = P_MUSTERI_ID
         ORDER BY ADRES_ID DESC;
    END PRC_MUSTERI_ADRESLERI;

END PKG_MUSTERI;
/

-- ============================================================
-- PACKAGE SPECIFICATION: PKG_HESAP
-- ============================================================

CREATE OR REPLACE PACKAGE PKG_HESAP AS

    PROCEDURE PRC_HESAP_EKLE(
        P_HESAP_NO     IN MVD_HESAP.HESAP_NO%TYPE,
        P_MUSTERI_ID   IN MVD_HESAP.MUSTERI_ID%TYPE,
        P_IBAN         IN MVD_HESAP.IBAN%TYPE,
        P_HESAP_TURU   IN MVD_HESAP.HESAP_TURU%TYPE,
        P_DOVIZ_CINSI  IN MVD_HESAP.DOVIZ_CINSI%TYPE,
        P_BAKIYE       IN MVD_HESAP.BAKIYE%TYPE,
        P_DURUM        IN MVD_HESAP.DURUM%TYPE
    );

    PROCEDURE PRC_HESAP_DURUM_GUNCELLE(
        P_HESAP_NO     IN MVD_HESAP.HESAP_NO%TYPE,
        P_DURUM        IN MVD_HESAP.DURUM%TYPE
    );

    PROCEDURE PRC_HAREKET_EKLE(
        P_HESAP_NO     IN MVD_HESAPHAREKET.HESAP_NO%TYPE,
        P_ISLEM_YONU   IN MVD_HESAPHAREKET.ISLEM_YONU%TYPE,
        P_ISLEM_TUTARI IN MVD_HESAPHAREKET.ISLEM_TUTARI%TYPE,
        P_DOVIZ_CINSI  IN MVD_HESAPHAREKET.DOVIZ_CINSI%TYPE,
        P_ACIKLAMA     IN MVD_HESAPHAREKET.ACIKLAMA%TYPE,
        P_ISLEM_KODU   IN MVD_HESAPHAREKET.ISLEM_KODU%TYPE,
        P_REFERANS_NO  IN MVD_HESAPHAREKET.REFERANS_NO%TYPE
    );

    -- Iki hesap arasinda para transferi. Is kurallari ihlal edildiginde exception
    -- firlatmaz; P_ISLEM_KODU ('0' = basarili) ve P_HATA_MESAJI ile bilgi doner.
    -- Transaction (COMMIT/ROLLBACK) cagiran katmana (BankAPI) aittir.
    PROCEDURE PRC_PARA_TRANSFERI(
        P_GONDEREN_IBAN     IN  MVD_HESAP.IBAN%TYPE,
        P_ALICI_IBAN        IN  MVD_HESAP.IBAN%TYPE,
        P_TUTAR             IN  MVD_HESAPHAREKET.ISLEM_TUTARI%TYPE,
        P_ACIKLAMA          IN  MVD_HESAPHAREKET.ACIKLAMA%TYPE,
        P_ISLEM_KODU        OUT VARCHAR2,
        P_HATA_MESAJI       OUT VARCHAR2,
        P_REFERANS_NO       OUT MVD_HESAPHAREKET.REFERANS_NO%TYPE,
        P_GONDEREN_ISLEM_ID OUT MVD_HESAPHAREKET.ISLEM_ID%TYPE,
        P_ALICI_ISLEM_ID    OUT MVD_HESAPHAREKET.ISLEM_ID%TYPE
    );

    PROCEDURE PRC_MUSTERI_HESAPLARI(
        P_MUSTERI_ID   IN  MVD_HESAP.MUSTERI_ID%TYPE,
        P_RESULT       OUT SYS_REFCURSOR
    );

    PROCEDURE PRC_HESAP_HAREKETLERI(
        P_HESAP_NO     IN  MVD_HESAPHAREKET.HESAP_NO%TYPE,
        P_RESULT       OUT SYS_REFCURSOR
    );

    PROCEDURE PRC_HESAP_GETIR(
        P_HESAP_NO     IN  MVD_HESAP.HESAP_NO%TYPE,
        P_RESULT       OUT SYS_REFCURSOR
    );

    PROCEDURE PRC_HESAP_LISTE(
        P_SEARCH_TERM     IN  VARCHAR2 DEFAULT NULL,
        P_MUSTERI_TIPI    IN  NUMBER DEFAULT NULL,
        P_HESAP_TURU      IN  VARCHAR2 DEFAULT NULL,
        P_DOVIZ_CINSI     IN  VARCHAR2 DEFAULT NULL,
        P_DURUM           IN  NUMBER DEFAULT NULL,
        P_MIN_BAKIYE      IN  NUMBER DEFAULT NULL,
        P_MAX_BAKIYE      IN  NUMBER DEFAULT NULL,
        P_HESAP_NO        IN  VARCHAR2 DEFAULT NULL,
        P_MUSTERI_ADI     IN  VARCHAR2 DEFAULT NULL,
        P_MUSTERI_SOYADI  IN  VARCHAR2 DEFAULT NULL,
        P_RESULT          OUT SYS_REFCURSOR
    );

    PROCEDURE PRC_HAREKET_LISTE(
        P_SEARCH_TERM      IN  VARCHAR2 DEFAULT NULL,
        P_ISLEM_YONU       IN  VARCHAR2 DEFAULT NULL,
        P_DOVIZ_CINSI      IN  VARCHAR2 DEFAULT NULL,
        P_BASLANGIC_TARIHI IN  DATE DEFAULT NULL,
        P_BITIS_TARIHI     IN  DATE DEFAULT NULL,
        P_MIN_TUTAR        IN  NUMBER DEFAULT NULL,
        P_MAX_TUTAR        IN  NUMBER DEFAULT NULL,
        P_HESAP_NO         IN  VARCHAR2 DEFAULT NULL,
        P_MUSTERI_ADI      IN  VARCHAR2 DEFAULT NULL,
        P_MUSTERI_SOYADI   IN  VARCHAR2 DEFAULT NULL,
        P_RESULT           OUT SYS_REFCURSOR
    );

    PROCEDURE PRC_HAREKET_GETIR(
        P_ISLEM_ID     IN  MVD_HESAPHAREKET.ISLEM_ID%TYPE,
        P_RESULT       OUT SYS_REFCURSOR
    );

END PKG_HESAP;
/


-- ============================================================
-- PACKAGE BODY: PKG_HESAP
-- ============================================================

CREATE OR REPLACE PACKAGE BODY PKG_HESAP AS

    -- Liste prosedurlerinin dondurecegi azami satir sayisi. Buyuk veri
    -- senaryosunda arayuze tum tabloyu tasimamak icin kullanilir.
    C_MAX_SATIR CONSTANT NUMBER := 10000;

    PROCEDURE PRC_HESAP_EKLE(
        P_HESAP_NO     IN MVD_HESAP.HESAP_NO%TYPE,
        P_MUSTERI_ID   IN MVD_HESAP.MUSTERI_ID%TYPE,
        P_IBAN         IN MVD_HESAP.IBAN%TYPE,
        P_HESAP_TURU   IN MVD_HESAP.HESAP_TURU%TYPE,
        P_DOVIZ_CINSI  IN MVD_HESAP.DOVIZ_CINSI%TYPE,
        P_BAKIYE       IN MVD_HESAP.BAKIYE%TYPE,
        P_DURUM        IN MVD_HESAP.DURUM%TYPE
    ) IS
        V_MUSTERI_SAY NUMBER;
        V_HESAP_SAY   NUMBER;
    BEGIN
        IF TRIM(P_HESAP_NO) IS NULL THEN
            RAISE_APPLICATION_ERROR(-20010, 'Hesap numarasi bos olamaz.');
        END IF;

        SELECT COUNT(1) INTO V_MUSTERI_SAY
          FROM MST_MUSTERI
         WHERE MUSTERI_ID = P_MUSTERI_ID;

        IF V_MUSTERI_SAY = 0 THEN
            RAISE_APPLICATION_ERROR(-20011, 'Musteri kaydi bulunamadi.');
        END IF;

        SELECT COUNT(1) INTO V_HESAP_SAY
          FROM MVD_HESAP
         WHERE HESAP_NO = P_HESAP_NO;

        IF V_HESAP_SAY > 0 THEN
            RAISE_APPLICATION_ERROR(-20012, 'Ayni hesap numarasi ile baska hesap mevcut.');
        END IF;

        INSERT INTO MVD_HESAP (
            HESAP_NO, MUSTERI_ID, IBAN, HESAP_TURU, DOVIZ_CINSI, BAKIYE, DURUM
        ) VALUES (
            P_HESAP_NO, P_MUSTERI_ID, P_IBAN, P_HESAP_TURU, P_DOVIZ_CINSI, P_BAKIYE, P_DURUM
        );
    END PRC_HESAP_EKLE;

    PROCEDURE PRC_HESAP_DURUM_GUNCELLE(
        P_HESAP_NO     IN MVD_HESAP.HESAP_NO%TYPE,
        P_DURUM        IN MVD_HESAP.DURUM%TYPE
    ) IS
    BEGIN
        UPDATE MVD_HESAP
           SET DURUM = P_DURUM
         WHERE HESAP_NO = P_HESAP_NO;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20013, 'Hesap bulunamadi.');
        END IF;
    END PRC_HESAP_DURUM_GUNCELLE;

    PROCEDURE PRC_HAREKET_EKLE(
        P_HESAP_NO     IN MVD_HESAPHAREKET.HESAP_NO%TYPE,
        P_ISLEM_YONU   IN MVD_HESAPHAREKET.ISLEM_YONU%TYPE,
        P_ISLEM_TUTARI IN MVD_HESAPHAREKET.ISLEM_TUTARI%TYPE,
        P_DOVIZ_CINSI  IN MVD_HESAPHAREKET.DOVIZ_CINSI%TYPE,
        P_ACIKLAMA     IN MVD_HESAPHAREKET.ACIKLAMA%TYPE,
        P_ISLEM_KODU   IN MVD_HESAPHAREKET.ISLEM_KODU%TYPE,
        P_REFERANS_NO  IN MVD_HESAPHAREKET.REFERANS_NO%TYPE
    ) IS
    BEGIN
        -- Hareketi ekle — TRG_MVD_HESAPHAREKET_BAKIYE trigger'i
        -- otomatik olarak bakiyeyi hesaplar ve MVD_HESAP'i gunceller.
        -- Tum kontroller ve kilit mekanizmasi trigger'da islenir.
        INSERT INTO MVD_HESAPHAREKET (
            HESAP_NO, ISLEM_YONU, ISLEM_TUTARI, DOVIZ_CINSI,
            YENI_BAKIYE, ISLEM_TARIHI, ACIKLAMA, ISLEM_KODU, REFERANS_NO
        ) VALUES (
            P_HESAP_NO, P_ISLEM_YONU, P_ISLEM_TUTARI, P_DOVIZ_CINSI,
            0, SYSTIMESTAMP, P_ACIKLAMA, P_ISLEM_KODU, P_REFERANS_NO
        );
    END PRC_HAREKET_EKLE;

    PROCEDURE PRC_PARA_TRANSFERI(
        P_GONDEREN_IBAN     IN  MVD_HESAP.IBAN%TYPE,
        P_ALICI_IBAN        IN  MVD_HESAP.IBAN%TYPE,
        P_TUTAR             IN  MVD_HESAPHAREKET.ISLEM_TUTARI%TYPE,
        P_ACIKLAMA          IN  MVD_HESAPHAREKET.ACIKLAMA%TYPE,
        P_ISLEM_KODU        OUT VARCHAR2,
        P_HATA_MESAJI       OUT VARCHAR2,
        P_REFERANS_NO       OUT MVD_HESAPHAREKET.REFERANS_NO%TYPE,
        P_GONDEREN_ISLEM_ID OUT MVD_HESAPHAREKET.ISLEM_ID%TYPE,
        P_ALICI_ISLEM_ID    OUT MVD_HESAPHAREKET.ISLEM_ID%TYPE
    ) IS
        V_GONDEREN_IBAN     MVD_HESAP.IBAN%TYPE := UPPER(TRIM(P_GONDEREN_IBAN));
        V_ALICI_IBAN        MVD_HESAP.IBAN%TYPE := UPPER(TRIM(P_ALICI_IBAN));

        V_GONDEREN_HESAP_NO MVD_HESAP.HESAP_NO%TYPE;
        V_GONDEREN_BAKIYE   MVD_HESAP.BAKIYE%TYPE;
        V_GONDEREN_DURUM    MVD_HESAP.DURUM%TYPE;
        V_GONDEREN_DOVIZ    MVD_HESAP.DOVIZ_CINSI%TYPE;

        V_ALICI_HESAP_NO    MVD_HESAP.HESAP_NO%TYPE;
        V_ALICI_BAKIYE      MVD_HESAP.BAKIYE%TYPE;
        V_ALICI_DURUM       MVD_HESAP.DURUM%TYPE;
        V_ALICI_DOVIZ       MVD_HESAP.DOVIZ_CINSI%TYPE;
    BEGIN
        P_ISLEM_KODU  := '0';
        P_HATA_MESAJI := 'Islem basarili.';

        -- KURAL 0: Zorunlu alanlar
        IF V_GONDEREN_IBAN IS NULL OR V_ALICI_IBAN IS NULL OR P_TUTAR IS NULL THEN
            P_ISLEM_KODU  := '100';
            P_HATA_MESAJI := 'Gonderen IBAN, alici IBAN ve tutar zorunludur.';
            RETURN;
        END IF;

        -- KURAL 1: Gonderen ve alici IBAN ayni olamaz
        IF V_GONDEREN_IBAN = V_ALICI_IBAN THEN
            P_ISLEM_KODU  := '101';
            P_HATA_MESAJI := 'Gonderen ve alici IBAN ayni olamaz.';
            RETURN;
        END IF;

        -- KURAL 2: Tutar sifirdan buyuk olmali
        IF P_TUTAR <= 0 THEN
            P_ISLEM_KODU  := '102';
            P_HATA_MESAJI := 'Transfer tutari 0 dan buyuk olmalidir.';
            RETURN;
        END IF;

        -- KURAL 3: Gonderen hesap bilgileri
        BEGIN
            SELECT HESAP_NO, BAKIYE, DURUM, DOVIZ_CINSI
              INTO V_GONDEREN_HESAP_NO, V_GONDEREN_BAKIYE, V_GONDEREN_DURUM, V_GONDEREN_DOVIZ
              FROM MVD_HESAP
             WHERE IBAN = V_GONDEREN_IBAN;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                P_ISLEM_KODU  := '103';
                P_HATA_MESAJI := 'Gonderen IBAN bulunamadi.';
                RETURN;
        END;

        -- KURAL 4: Alici hesap bilgileri
        BEGIN
            SELECT HESAP_NO, BAKIYE, DURUM, DOVIZ_CINSI
              INTO V_ALICI_HESAP_NO, V_ALICI_BAKIYE, V_ALICI_DURUM, V_ALICI_DOVIZ
              FROM MVD_HESAP
             WHERE IBAN = V_ALICI_IBAN;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                P_ISLEM_KODU  := '104';
                P_HATA_MESAJI := 'Alici IBAN bulunamadi.';
                RETURN;
        END;

        -- KURAL 5: Hesaplar aktif olmali (1: Aktif)
        IF V_GONDEREN_DURUM <> 1 THEN
            P_ISLEM_KODU  := '105';
            P_HATA_MESAJI := 'Gonderen hesap aktif degil.';
            RETURN;
        END IF;

        IF V_ALICI_DURUM <> 1 THEN
            P_ISLEM_KODU  := '106';
            P_HATA_MESAJI := 'Alici hesap aktif degil.';
            RETURN;
        END IF;

        -- KURAL 6: Kur cevrimi olmadigi icin doviz cinsleri ayni olmali
        IF V_GONDEREN_DOVIZ <> V_ALICI_DOVIZ THEN
            P_ISLEM_KODU  := '107';
            P_HATA_MESAJI := 'Farkli doviz cinsleri arasinda transfer yapilamaz.';
            RETURN;
        END IF;

        -- Iki hesap da her zaman HESAP_NO sirasina gore kilitlenir; boylece
        -- ayni anda calisan A->B ve B->A transferleri deadlock uretmez.
        IF V_GONDEREN_HESAP_NO < V_ALICI_HESAP_NO THEN
            SELECT BAKIYE INTO V_GONDEREN_BAKIYE FROM MVD_HESAP WHERE HESAP_NO = V_GONDEREN_HESAP_NO FOR UPDATE;
            SELECT BAKIYE INTO V_ALICI_BAKIYE    FROM MVD_HESAP WHERE HESAP_NO = V_ALICI_HESAP_NO    FOR UPDATE;
        ELSE
            SELECT BAKIYE INTO V_ALICI_BAKIYE    FROM MVD_HESAP WHERE HESAP_NO = V_ALICI_HESAP_NO    FOR UPDATE;
            SELECT BAKIYE INTO V_GONDEREN_BAKIYE FROM MVD_HESAP WHERE HESAP_NO = V_GONDEREN_HESAP_NO FOR UPDATE;
        END IF;

        -- KURAL 7: Bakiye kontrolu (kilit alindiktan sonraki guncel bakiye ile)
        IF V_GONDEREN_BAKIYE < P_TUTAR THEN
            P_ISLEM_KODU  := '108';
            P_HATA_MESAJI := 'Gonderen hesap bakiyesi yetersiz.';
            RETURN;
        END IF;

        -- Iki hareketi de birbirine baglayan referans numarasi
        P_REFERANS_NO := 'TRF' || TO_CHAR(SYSTIMESTAMP, 'YYYYMMDDHH24MISSFF3')
                               || LPAD(TRUNC(DBMS_RANDOM.VALUE(0, 10000)), 4, '0');

        -- Bakiyeler burada ELLE guncellenmez: MVD_HESAP.BAKIYE'yi
        -- TRG_MVD_HESAPHAREKET_BAKIYE trigger'i hareket INSERT'inde gunceller.
        -- Ayrica UPDATE yapilirsa tutar iki kez islenmis olur.

        -- Gonderen: para cikisi (C)
        INSERT INTO MVD_HESAPHAREKET (
            HESAP_NO, ISLEM_YONU, ISLEM_TUTARI, DOVIZ_CINSI,
            YENI_BAKIYE, ISLEM_TARIHI, ACIKLAMA, ISLEM_KODU, REFERANS_NO
        ) VALUES (
            V_GONDEREN_HESAP_NO, 'C', P_TUTAR, V_GONDEREN_DOVIZ,
            0, SYSTIMESTAMP, P_ACIKLAMA, 'TRF_CIKIS', P_REFERANS_NO
        )
        RETURNING ISLEM_ID INTO P_GONDEREN_ISLEM_ID;

        -- Alici: para girisi (B)
        INSERT INTO MVD_HESAPHAREKET (
            HESAP_NO, ISLEM_YONU, ISLEM_TUTARI, DOVIZ_CINSI,
            YENI_BAKIYE, ISLEM_TARIHI, ACIKLAMA, ISLEM_KODU, REFERANS_NO
        ) VALUES (
            V_ALICI_HESAP_NO, 'B', P_TUTAR, V_ALICI_DOVIZ,
            0, SYSTIMESTAMP, P_ACIKLAMA, 'TRF_GIRIS', P_REFERANS_NO
        )
        RETURNING ISLEM_ID INTO P_ALICI_ISLEM_ID;
    EXCEPTION
        WHEN OTHERS THEN
            -- Burada ROLLBACK yapilmaz; transaction'i acan katman (BankAPI)
            -- islem kodunu gorup ROLLBACK/COMMIT kararini kendisi verir.
            P_ISLEM_KODU        := '500';
            P_HATA_MESAJI       := 'Veritabani hatasi: ' || SUBSTR(SQLERRM, 1, 200);
            P_REFERANS_NO       := NULL;
            P_GONDEREN_ISLEM_ID := NULL;
            P_ALICI_ISLEM_ID    := NULL;
    END PRC_PARA_TRANSFERI;

    PROCEDURE PRC_MUSTERI_HESAPLARI(
        P_MUSTERI_ID   IN  MVD_HESAP.MUSTERI_ID%TYPE,
        P_RESULT       OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN P_RESULT FOR
        SELECT HESAP_NO, MUSTERI_ID, IBAN, HESAP_TURU, DOVIZ_CINSI, BAKIYE, DURUM
          FROM MVD_HESAP
         WHERE MUSTERI_ID = P_MUSTERI_ID
         ORDER BY HESAP_NO;
    END PRC_MUSTERI_HESAPLARI;

    PROCEDURE PRC_HESAP_HAREKETLERI(
        P_HESAP_NO     IN  MVD_HESAPHAREKET.HESAP_NO%TYPE,
        P_RESULT       OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN P_RESULT FOR
        SELECT ISLEM_ID, HESAP_NO, ISLEM_YONU, ISLEM_TUTARI, DOVIZ_CINSI, 
               YENI_BAKIYE, ISLEM_TARIHI, ACIKLAMA, ISLEM_KODU, REFERANS_NO
          FROM MVD_HESAPHAREKET
         WHERE HESAP_NO = P_HESAP_NO
         ORDER BY ISLEM_TARIHI DESC;
    END PRC_HESAP_HAREKETLERI;

    PROCEDURE PRC_HESAP_GETIR(
        P_HESAP_NO     IN  MVD_HESAP.HESAP_NO%TYPE,
        P_RESULT       OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN P_RESULT FOR
        SELECT HESAP_NO, MUSTERI_ID, IBAN, HESAP_TURU, DOVIZ_CINSI, BAKIYE, DURUM
          FROM MVD_HESAP
         WHERE HESAP_NO = P_HESAP_NO;
    END PRC_HESAP_GETIR;

    PROCEDURE PRC_HESAP_LISTE(
        P_SEARCH_TERM     IN  VARCHAR2 DEFAULT NULL,
        P_MUSTERI_TIPI    IN  NUMBER DEFAULT NULL,
        P_HESAP_TURU      IN  VARCHAR2 DEFAULT NULL,
        P_DOVIZ_CINSI     IN  VARCHAR2 DEFAULT NULL,
        P_DURUM           IN  NUMBER DEFAULT NULL,
        P_MIN_BAKIYE      IN  NUMBER DEFAULT NULL,
        P_MAX_BAKIYE      IN  NUMBER DEFAULT NULL,
        P_HESAP_NO        IN  VARCHAR2 DEFAULT NULL,
        P_MUSTERI_ADI     IN  VARCHAR2 DEFAULT NULL,
        P_MUSTERI_SOYADI  IN  VARCHAR2 DEFAULT NULL,
        P_RESULT          OUT SYS_REFCURSOR
    ) IS
    BEGIN
        -- Filtreleme tamamen DB tarafinda yapilir; arayuz "Uygula" ile
        -- yalnizca kriterlere uyan kayitlari ister (buyuk veri senaryosu).
        OPEN P_RESULT FOR
        SELECT H.HESAP_NO, H.MUSTERI_ID, H.IBAN, H.HESAP_TURU,
               H.DOVIZ_CINSI, H.BAKIYE, H.DURUM
          FROM MVD_HESAP H
          JOIN MST_MUSTERI M
            ON M.MUSTERI_ID = H.MUSTERI_ID
         WHERE (P_SEARCH_TERM IS NULL OR (
                   UPPER(H.HESAP_NO) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   UPPER(H.IBAN) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   UPPER(H.HESAP_TURU) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   UPPER(H.DOVIZ_CINSI) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   UPPER(M.AD) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   UPPER(M.SOYAD) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   UPPER(TRIM(M.AD) || ' ' || TRIM(M.SOYAD)) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   TO_CHAR(H.MUSTERI_ID) LIKE '%' || P_SEARCH_TERM || '%'
               ))
           AND (P_MUSTERI_TIPI IS NULL OR M.MUSTERI_TIPI = P_MUSTERI_TIPI)
           AND (P_HESAP_TURU IS NULL OR UPPER(H.HESAP_TURU) = UPPER(P_HESAP_TURU))
           AND (P_DOVIZ_CINSI IS NULL OR UPPER(H.DOVIZ_CINSI) = UPPER(P_DOVIZ_CINSI)
                OR (UPPER(P_DOVIZ_CINSI) = 'TRY' AND UPPER(H.DOVIZ_CINSI) = 'TL'))
           AND (P_DURUM IS NULL OR H.DURUM = P_DURUM)
           AND (P_MIN_BAKIYE IS NULL OR H.BAKIYE >= P_MIN_BAKIYE)
           AND (P_MAX_BAKIYE IS NULL OR H.BAKIYE <= P_MAX_BAKIYE)
           -- Kolon bazli filtreler: ID alani hesap no veya musteri ID ile eslesir
           AND (P_HESAP_NO IS NULL OR (
                   UPPER(H.HESAP_NO) LIKE '%' || UPPER(P_HESAP_NO) || '%' OR
                   TO_CHAR(H.MUSTERI_ID) LIKE '%' || P_HESAP_NO || '%'
               ))
           AND (P_MUSTERI_ADI IS NULL OR UPPER(M.AD) LIKE '%' || UPPER(P_MUSTERI_ADI) || '%')
           AND (P_MUSTERI_SOYADI IS NULL OR UPPER(M.SOYAD) LIKE '%' || UPPER(P_MUSTERI_SOYADI) || '%')
         ORDER BY H.HESAP_NO
         FETCH FIRST C_MAX_SATIR ROWS ONLY;
    END PRC_HESAP_LISTE;

    PROCEDURE PRC_HAREKET_LISTE(
        P_SEARCH_TERM      IN  VARCHAR2 DEFAULT NULL,
        P_ISLEM_YONU       IN  VARCHAR2 DEFAULT NULL,
        P_DOVIZ_CINSI      IN  VARCHAR2 DEFAULT NULL,
        P_BASLANGIC_TARIHI IN  DATE DEFAULT NULL,
        P_BITIS_TARIHI     IN  DATE DEFAULT NULL,
        P_MIN_TUTAR        IN  NUMBER DEFAULT NULL,
        P_MAX_TUTAR        IN  NUMBER DEFAULT NULL,
        P_HESAP_NO         IN  VARCHAR2 DEFAULT NULL,
        P_MUSTERI_ADI      IN  VARCHAR2 DEFAULT NULL,
        P_MUSTERI_SOYADI   IN  VARCHAR2 DEFAULT NULL,
        P_RESULT           OUT SYS_REFCURSOR
    ) IS
    BEGIN
        -- Filtreleme tamamen DB tarafinda yapilir; arayuz "Uygula" ile
        -- yalnizca kriterlere uyan kayitlari ister (buyuk veri senaryosu).
        -- P_ISLEM_YONU: 'B' = para girisi, 'C' = para cikisi
        -- Musteri adi/soyadi filtresi icin hareket -> hesap -> musteri join'i kullanilir.
        OPEN P_RESULT FOR
        SELECT HH.ISLEM_ID, HH.HESAP_NO, HH.ISLEM_YONU, HH.ISLEM_TUTARI, HH.DOVIZ_CINSI,
               HH.YENI_BAKIYE, HH.ISLEM_TARIHI, HH.ACIKLAMA, HH.ISLEM_KODU, HH.REFERANS_NO,
               M.MUSTERI_ID, M.AD AS MUSTERI_ADI, M.SOYAD AS MUSTERI_SOYADI
          FROM MVD_HESAPHAREKET HH
          JOIN MVD_HESAP H
            ON H.HESAP_NO = HH.HESAP_NO
          JOIN MST_MUSTERI M
            ON M.MUSTERI_ID = H.MUSTERI_ID
         WHERE (P_SEARCH_TERM IS NULL OR (
                   UPPER(HH.HESAP_NO) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   UPPER(HH.ACIKLAMA) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   UPPER(HH.REFERANS_NO) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   UPPER(HH.ISLEM_KODU) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   UPPER(HH.DOVIZ_CINSI) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   UPPER(M.AD) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   UPPER(M.SOYAD) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   UPPER(TRIM(M.AD) || ' ' || TRIM(M.SOYAD)) LIKE '%' || UPPER(P_SEARCH_TERM) || '%' OR
                   TO_CHAR(HH.ISLEM_ID) LIKE '%' || P_SEARCH_TERM || '%'
               ))
           AND (P_ISLEM_YONU IS NULL OR UPPER(HH.ISLEM_YONU) = UPPER(P_ISLEM_YONU))
           AND (P_DOVIZ_CINSI IS NULL OR UPPER(HH.DOVIZ_CINSI) = UPPER(P_DOVIZ_CINSI)
                OR (UPPER(P_DOVIZ_CINSI) = 'TRY' AND UPPER(HH.DOVIZ_CINSI) = 'TL'))
           AND (P_BASLANGIC_TARIHI IS NULL OR HH.ISLEM_TARIHI >= TRUNC(P_BASLANGIC_TARIHI))
           -- Bitis tarihi dahil olmali: gunun sonuna kadar (TRUNC + 1 gun)
           AND (P_BITIS_TARIHI IS NULL OR HH.ISLEM_TARIHI < TRUNC(P_BITIS_TARIHI) + 1)
           AND (P_MIN_TUTAR IS NULL OR HH.ISLEM_TUTARI >= P_MIN_TUTAR)
           AND (P_MAX_TUTAR IS NULL OR HH.ISLEM_TUTARI <= P_MAX_TUTAR)
           -- Kolon bazli filtreler: hesap no ve musteri adi/soyadi
           AND (P_HESAP_NO IS NULL OR UPPER(HH.HESAP_NO) LIKE '%' || UPPER(P_HESAP_NO) || '%')
           AND (P_MUSTERI_ADI IS NULL OR UPPER(M.AD) LIKE '%' || UPPER(P_MUSTERI_ADI) || '%')
           AND (P_MUSTERI_SOYADI IS NULL OR UPPER(M.SOYAD) LIKE '%' || UPPER(P_MUSTERI_SOYADI) || '%')
         ORDER BY HH.ISLEM_TARIHI DESC
         FETCH FIRST C_MAX_SATIR ROWS ONLY;
    END PRC_HAREKET_LISTE;

    PROCEDURE PRC_HAREKET_GETIR(
        P_ISLEM_ID     IN  MVD_HESAPHAREKET.ISLEM_ID%TYPE,
        P_RESULT       OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN P_RESULT FOR
        SELECT ISLEM_ID, HESAP_NO, ISLEM_YONU, ISLEM_TUTARI, DOVIZ_CINSI, 
               YENI_BAKIYE, ISLEM_TARIHI, ACIKLAMA, ISLEM_KODU, REFERANS_NO
          FROM MVD_HESAPHAREKET
         WHERE ISLEM_ID = P_ISLEM_ID;
    END PRC_HAREKET_GETIR;

END PKG_HESAP;
/




-- ============================================================
-- PACKAGE SPECIFICATION: PKG_DASHBOARD
-- ============================================================
CREATE OR REPLACE PACKAGE PKG_DASHBOARD AS
    PROCEDURE PRC_GET_SUMMARY(
        P_MUSTERI_STATS OUT SYS_REFCURSOR,
        P_HESAP_STATS   OUT SYS_REFCURSOR,
        P_HACIM_STATS   OUT SYS_REFCURSOR,
        P_SON_ISLEMLER  OUT SYS_REFCURSOR
    );
END PKG_DASHBOARD;
/

-- ============================================================
-- PACKAGE BODY: PKG_DASHBOARD
-- ============================================================
CREATE OR REPLACE PACKAGE BODY PKG_DASHBOARD AS
    PROCEDURE PRC_GET_SUMMARY(
        P_MUSTERI_STATS OUT SYS_REFCURSOR,
        P_HESAP_STATS   OUT SYS_REFCURSOR,
        P_HACIM_STATS   OUT SYS_REFCURSOR,
        P_SON_ISLEMLER  OUT SYS_REFCURSOR
    ) IS
    BEGIN
        -- 1. Toplam Müşteri İstatistikleri
        OPEN P_MUSTERI_STATS FOR
        SELECT 
            SUM(CASE WHEN AKTIF_MI = 1 THEN 1 ELSE 0 END) AS AKTIF_MUSTERI_SAYISI,
            SUM(CASE WHEN MUSTERI_TIPI = 1 THEN 1 ELSE 0 END) AS BIREYSEL_SAYISI,
            SUM(CASE WHEN MUSTERI_TIPI = 2 THEN 1 ELSE 0 END) AS TUZEL_SAYISI,
            COUNT(1) AS TOPLAM_MUSTERI
        FROM MST_MUSTERI;

        -- 2. Hesap İstatistikleri (Döviz bazlı toplam bakiyeler)
        OPEN P_HESAP_STATS FOR
        SELECT 
            DOVIZ_CINSI,
            SUM(BAKIYE) AS TOPLAM_BAKIYE,
            COUNT(1) AS HESAP_SAYISI,
            SUM(CASE WHEN HESAP_TURU = 'Vadesiz' THEN 1 ELSE 0 END) AS VADESIZ_SAYISI,
            SUM(CASE WHEN HESAP_TURU = 'Vadeli' THEN 1 ELSE 0 END) AS VADELI_SAYISI
        FROM MVD_HESAP
        WHERE DURUM = 1
        GROUP BY DOVIZ_CINSI;

        -- 3. Toplam İşlem Hacmi (Tüm hesap hareketlerinin döviz ve işlem yönü bazlı hacmi)
        OPEN P_HACIM_STATS FOR
        SELECT 
            DOVIZ_CINSI,
            ISLEM_YONU,
            SUM(ISLEM_TUTARI) AS TOPLAM_HACIM,
            COUNT(1) AS ISLEM_SAYISI
        FROM MVD_HESAPHAREKET
        GROUP BY DOVIZ_CINSI, ISLEM_YONU;


        -- 4. Son 10 İşlem
        OPEN P_SON_ISLEMLER FOR
        SELECT ISLEM_ID, HESAP_NO, ISLEM_YONU, ISLEM_TUTARI, DOVIZ_CINSI, 
               YENI_BAKIYE, ISLEM_TARIHI, ACIKLAMA
        FROM MVD_HESAPHAREKET
        ORDER BY ISLEM_TARIHI DESC
        FETCH FIRST 10 ROWS ONLY;

    END PRC_GET_SUMMARY;
END PKG_DASHBOARD;
/
