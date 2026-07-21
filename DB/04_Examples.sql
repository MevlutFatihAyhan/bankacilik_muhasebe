-- ============================================================
--  04_EXAMPLES.SQL
--  Örnek INSERT, UPDATE, DELETE ve SELECT işlemleri
--  Çalıştırma sırası: 03_Procedures.sql'den sonra çalıştırılmalıdır.
-- ============================================================
--  NOT: DURUM değerleri → AKTIF / PASIF / KAPALI
--       ISLEM_YONU       → B (Para Girisi) / C (Para Cikisi)
-- ============================================================

SET SERVEROUTPUT ON;

-- ============================================================
-- ÖRNEK 1: Müşteri Ekleme (PKG_MUSTERI.PRC_MUSTERI_EKLE)
-- ============================================================

DECLARE
    v_musteri_id MST_MUSTERI.MUSTERI_ID%TYPE;
BEGIN
    DBMS_OUTPUT.PUT_LINE('--- ÖRNEK 1: Musteri Ekleme ---');

    PKG_MUSTERI.PRC_MUSTERI_EKLE(
        P_AD           => 'Ahmet',
        P_SOYAD        => 'Yilmaz',
        P_MUSTERI_TIPI => 1,             -- 1: Bireysel, 2: Tuzel
        P_KIMLIK_NO    => '11111111111', -- 11 haneli TC Kimlik No
        P_EMAIL        => 'ahmet.yilmaz@ornek.com',
        P_TELEFON      => '05551234567',
        P_AKTIF_MI     => 1,
        P_MUSTERI_ID   => v_musteri_id
    );

    DBMS_OUTPUT.PUT_LINE('Musteri eklendi. MUSTERI_ID: ' || v_musteri_id);
END;
/

-- ============================================================
-- ÖRNEK 2: Müşteri Güncelleme (PKG_MUSTERI.PRC_MUSTERI_GUNCELLE)
-- ============================================================

DECLARE
    v_musteri_id MST_MUSTERI.MUSTERI_ID%TYPE;
BEGIN
    DBMS_OUTPUT.PUT_LINE('--- ÖRNEK 2: Musteri Guncelleme ---');

    SELECT MAX(MUSTERI_ID) INTO v_musteri_id FROM MST_MUSTERI;

    PKG_MUSTERI.PRC_MUSTERI_GUNCELLE(
        P_MUSTERI_ID   => v_musteri_id,
        P_AD           => 'Ahmet Fatih',
        P_SOYAD        => 'Yilmaz',
        P_MUSTERI_TIPI => 1,             -- 1: Bireysel, 2: Tuzel
        P_KIMLIK_NO    => '11111111111', -- ayni musteri, ayni kimlik no
        P_EMAIL        => 'ahmet.fatih@ornek.com',
        P_TELEFON      => '05559876543',
        P_AKTIF_MI     => 1
    );

    DBMS_OUTPUT.PUT_LINE('Musteri guncellendi. MUSTERI_ID: ' || v_musteri_id);
END;
/

-- ============================================================
-- ÖRNEK 3: Adres Ekleme (PKG_MUSTERI.PRC_ADRES_EKLE)
-- ============================================================

DECLARE
    v_musteri_id MST_MUSTERI.MUSTERI_ID%TYPE;
    v_adres_id   MST_MUSTERIADRES.ADRES_ID%TYPE;
BEGIN
    DBMS_OUTPUT.PUT_LINE('--- ÖRNEK 3: Adres Ekleme ---');

    SELECT MAX(MUSTERI_ID) INTO v_musteri_id FROM MST_MUSTERI;

    PKG_MUSTERI.PRC_ADRES_EKLE(
        P_MUSTERI_ID   => v_musteri_id,
        P_ADRES_BASLIK => 'Ev',
        P_ULKE         => 'Turkiye',
        P_SEHIR        => 'Istanbul',
        P_ILCE         => 'Kadikoy',
        P_POSTA_KODU   => '34710',
        P_ACIK_ADRES   => 'Moda Mahallesi No:10',
        P_ADRES_ID     => v_adres_id
    );

    DBMS_OUTPUT.PUT_LINE('Adres eklendi. ADRES_ID: ' || v_adres_id);
END;
/

-- ============================================================
-- ÖRNEK 4: Hesap Ekleme (PKG_HESAP.PRC_HESAP_EKLE)
-- NOT: DURUM tabloda NUMBER(1) olarak tanimlidir → 1: Aktif, 2: Pasif, 3: Kapali
--      (metin degerler ('AKTIF' vb.) gecersizdir, ORA-01722 hatasi verir)
-- ============================================================

DECLARE
    v_musteri_id MST_MUSTERI.MUSTERI_ID%TYPE;
BEGIN
    DBMS_OUTPUT.PUT_LINE('--- ÖRNEK 4: Hesap Ekleme ---');

    SELECT MAX(MUSTERI_ID) INTO v_musteri_id FROM MST_MUSTERI;

    PKG_HESAP.PRC_HESAP_EKLE(
        P_HESAP_NO    => 'TR01100001',
        P_MUSTERI_ID  => v_musteri_id,
        P_IBAN        => 'TR870006200000000010010001',
        P_HESAP_TURU  => 'Vadesiz',
        P_DOVIZ_CINSI => 'TRY',
        P_BAKIYE      => 5000.00,
        P_DURUM       => 1   -- Dogru: 1 (Aktif) / 2 (Pasif) / 3 (Kapali)
    );

    DBMS_OUTPUT.PUT_LINE('Hesap eklendi. HESAP_NO: TR01100001');
END;
/

-- ============================================================
-- ÖRNEK 5: Para Girişi — ISLEM_YONU = 'B' (sp_hesap_hareket_ekle)
-- ============================================================

DECLARE
BEGIN
    DBMS_OUTPUT.PUT_LINE('--- ÖRNEK 5: Para Girisi (B) ---');

    PKG_HESAP.PRC_HAREKET_EKLE(
        P_HESAP_NO     => 'TR01100001',
        P_ISLEM_YONU   => 'B',       -- B = Para Girisi (Borc)
        P_ISLEM_TUTARI => 1500.00,
        P_DOVIZ_CINSI  => 'TRY',
        P_ACIKLAMA     => 'EFT ile para girisi',
        P_ISLEM_KODU   => 'EFT',
        P_REFERANS_NO  => 'REF001'
    );

    DBMS_OUTPUT.PUT_LINE('Para girisi islendi. Yeni bakiye trigger tarafindan hesaplandi.');
END;
/

-- ============================================================
-- ÖRNEK 6: Para Çıkışı — ISLEM_YONU = 'C' (sp_hesap_hareket_ekle)
-- ============================================================

DECLARE
BEGIN
    DBMS_OUTPUT.PUT_LINE('--- ÖRNEK 6: Para Cikisi (C) ---');

    PKG_HESAP.PRC_HAREKET_EKLE(
        P_HESAP_NO     => 'TR01100001',
        P_ISLEM_YONU   => 'C',       -- C = Para Cikisi (Cari)
        P_ISLEM_TUTARI => 500.00,
        P_DOVIZ_CINSI  => 'TRY',
        P_ACIKLAMA     => 'ATM para cekimi',
        P_ISLEM_KODU   => 'ATM',
        P_REFERANS_NO  => 'REF002'
    );

    DBMS_OUTPUT.PUT_LINE('Para cikisi islendi. Yeni bakiye trigger tarafindan hesaplandi.');
END;
/

-- ============================================================
-- ÖRNEK 7: Hesap Durumu Güncelleme (Doğrudan UPDATE)
-- NOT: DURUM tabloda NUMBER(1) olarak tanimlidir → 1: Aktif, 2: Pasif, 3: Kapali
-- ============================================================

DECLARE
BEGIN
    DBMS_OUTPUT.PUT_LINE('--- ÖRNEK 7: Hesap Durumu Guncelleme ---');

    UPDATE MVD_HESAP
       SET DURUM = 2   -- 2: Pasif
     WHERE HESAP_NO = 'TR01100001';

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Hesap durumu PASIF yapildi.');
END;
/

-- ============================================================
-- ÖRNEK 8: Adres Güncelleme (PKG_MUSTERI.PRC_ADRES_GUNCELLE)
-- ============================================================

DECLARE
    v_adres_id MST_MUSTERIADRES.ADRES_ID%TYPE;
BEGIN
    DBMS_OUTPUT.PUT_LINE('--- ÖRNEK 8: Adres Guncelleme ---');

    SELECT MAX(ADRES_ID) INTO v_adres_id FROM MST_MUSTERIADRES;

    PKG_MUSTERI.PRC_ADRES_GUNCELLE(
        P_ADRES_ID     => v_adres_id,
        P_ADRES_BASLIK => 'Is Yeri',
        P_ULKE         => 'Turkiye',
        P_SEHIR        => 'Ankara',
        P_ILCE         => 'Cankaya',
        P_POSTA_KODU   => '06500',
        P_ACIK_ADRES   => 'Ataturk Bulvari No:20'
    );

    DBMS_OUTPUT.PUT_LINE('Adres guncellendi. ADRES_ID: ' || v_adres_id);
END;
/

-- ============================================================
-- ÖRNEK 9: Temizlik — Silme işlemleri (doğru sırayla)
-- Sıra: Hareketler → Hesap → Adres → Müşteri
-- ============================================================

DECLARE
    v_musteri_id MST_MUSTERI.MUSTERI_ID%TYPE;
BEGIN
    DBMS_OUTPUT.PUT_LINE('--- ÖRNEK 9: Temizlik (Silme) ---');

    SELECT MAX(MUSTERI_ID) INTO v_musteri_id FROM MST_MUSTERI;

    -- 1) Hesap hareketlerini sil
    DELETE FROM MVD_HESAPHAREKET
     WHERE HESAP_NO IN (
         SELECT HESAP_NO FROM MVD_HESAP WHERE MUSTERI_ID = v_musteri_id
     );
    DBMS_OUTPUT.PUT_LINE('Hareketler silindi.');

    -- 2) Hesapları sil
    DELETE FROM MVD_HESAP
     WHERE MUSTERI_ID = v_musteri_id;
    DBMS_OUTPUT.PUT_LINE('Hesaplar silindi.');

    -- 3) Adresleri sil (FK CASCADE ile otomatik silinir ama açıkça yazıldı)
    DELETE FROM MST_MUSTERIADRES
     WHERE MUSTERI_ID = v_musteri_id;
    DBMS_OUTPUT.PUT_LINE('Adresler silindi.');

    -- 4) Müşteriyi sil
    PKG_MUSTERI.PRC_MUSTERI_SIL(P_MUSTERI_ID => v_musteri_id);
    DBMS_OUTPUT.PUT_LINE('Musteri silindi. MUSTERI_ID: ' || v_musteri_id);
END;
/

-- ============================================================
-- ÖRNEK 10: SELECT — Güncel bakiye sorgulama
-- ============================================================

SELECT
    H.HESAP_NO,
    H.HESAP_TURU,
    H.DOVIZ_CINSI,
    H.BAKIYE,
    H.DURUM,
    M.AD || ' ' || M.SOYAD AS MUSTERI_ADI,
    M.EMAIL
FROM
    MVD_HESAP      H
    JOIN MST_MUSTERI M ON M.MUSTERI_ID = H.MUSTERI_ID
ORDER BY
    H.HESAP_NO;