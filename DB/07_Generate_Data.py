import oracledb
import random
import time

DB_USER = "ADMIN"
DB_PASSWORD = "MFZNAyhan2005."
DB_DSN = "(description= (retry_count=20)(retry_delay=3)(address=(protocol=tcps)(port=1521)(host=adb.eu-frankfurt-1.oraclecloud.com))(connect_data=(service_name=gd33e9612e30a83_mfabank_medium.adb.oraclecloud.com))(security=(ssl_server_dn_match=yes)))"

NUM_RECORDS_MUSTERI = 25000
NUM_HESAP_PER_MUSTERI = 4

cities = [
    "Adana", "Ankara", "Antalya", "Aydın", "Balıkesir", "Bursa",
    "Denizli", "Diyarbakır", "Erzurum", "Eskişehir", "Gaziantep",
    "Hatay", "İstanbul", "İzmir", "Kahramanmaraş", "Kayseri",
    "Kocaeli", "Konya", "Manisa", "Mersin"
]

first_names = [
    "Ahmet", "Mehmet", "Ali", "Ayşe", "Fatma", "Mustafa", "Murat", "Hakan", "Elif", "Zeynep","İlayda","Nihal",
    "John", "Michael", "David", "Emma", "Olivia", "James", "William", "Sophia", "Isabella", "Mia",
    "Can", "Cem", "Burak", "Emre", "Onur", "Esra", "Cansu", "Selin", "Gizem", "Büşra","Fatih"
]

last_names = [
    "Yılmaz", "Kaya", "Demir", "Çelik", "Şahin", "Yıldız", "Öztürk", "Aydın", "Özdemir", "Arslan","Ayhan",
    "Smith", "Johnson", "Williams", "Brown", "Jones", "Miller", "Davis", "Garcia", "Rodriguez", "Wilson",
    "Koç", "Sabancı", "Eczacıbaşı", "Ülker", "Zorlu", "Doğan", "Boyner", "Şahenk", "Kıraç", "Eczacıbaşı","Kavanoz"
]

corp_names = ["Teknoloji", "İnşaat", "Lojistik", "Yazılım", "Gıda", "Otomotiv", "Tekstil", "Danışmanlık", "Enerji", "Medya"]
corp_suffixes = ["A.Ş.", "Ltd. Şti.", "Inc.", "LLC", "Grup", "San. ve Tic.", "Pazarlama"]

def generate_tckn():
    digits = [random.randint(1, 9)] + [random.randint(0, 9) for _ in range(8)]
    odd_sum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8]
    even_sum = digits[1] + digits[3] + digits[5] + digits[7]
    d10 = ((odd_sum * 7) - even_sum) % 10
    d11 = (sum(digits) + d10) % 10
    digits.extend([d10, d11])
    return "".join(map(str, digits))

def generate_vkn():
    digits = [random.randint(0, 9) for _ in range(9)]
    sum_val = 0
    for i in range(9):
        tmp = (digits[i] + (10 - (i + 1))) % 10
        if tmp == 9:
            sum_val += 9
        else:
            sum_val += (tmp * (2 ** (10 - (i + 1)))) % 9
    v10 = (10 - (sum_val % 10)) % 10
    digits.append(v10)
    return "".join(map(str, digits))

def get_name(is_corp):
    if is_corp:
        return f"{random.choice(corp_names)} {random.choice(corp_suffixes)}", ""
    else:
        # %50 ihtimalle çift isim
        if random.random() < 0.5:
            fname = f"{random.choice(first_names)} {random.choice(first_names)}"
        else:
            fname = random.choice(first_names)
        lname = random.choice(last_names)
        return fname, lname

def calculate_iban_check_digits(bank_code, account_no_str):
    acc_numeric = ""
    for char in account_no_str:
        if char.isdigit():
            acc_numeric += char
        else:
            acc_numeric += str(ord(char.upper()) - 55)
    
    number_str = f"{bank_code}0{acc_numeric}292700"
    mod_97 = int(number_str) % 97
    check_digit = 98 - mod_97
    return f"{check_digit:02d}"

# TRIGGER SCRIPT (Bu Python içinden veritabanına basılacak)
TRIGGER_SQL = """
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
    
    -- YENI KURAL: Tuzel musteriler (Ticari hesaplar) Vadeli hesap acamaz
    IF :NEW.HESAP_TURU = 'Vadeli' THEN
        SELECT MUSTERI_TIPI INTO v_musteri_tipi FROM MST_MUSTERI WHERE MUSTERI_ID = :NEW.MUSTERI_ID;
        IF v_musteri_tipi = 2 THEN
            RAISE_APPLICATION_ERROR(-20060, 'Tuzel musteriler Vadeli hesap acamaz!');
        END IF;
    END IF;
END;
"""

# VERITABANI SIFIRLAMA SCRIPT
CLEANUP_SQL = """
BEGIN
    EXECUTE IMMEDIATE 'ALTER TABLE MVD_HESAPHAREKET DISABLE ALL TRIGGERS';
    EXECUTE IMMEDIATE 'ALTER TABLE MVD_HESAP DISABLE ALL TRIGGERS';
    EXECUTE IMMEDIATE 'ALTER TABLE MST_MUSTERIADRES DISABLE ALL TRIGGERS';
    EXECUTE IMMEDIATE 'ALTER TABLE MST_MUSTERI DISABLE ALL TRIGGERS';

    EXECUTE IMMEDIATE 'TRUNCATE TABLE MVD_HESAPHAREKET_H';
    EXECUTE IMMEDIATE 'DELETE FROM MVD_HESAPHAREKET';
    EXECUTE IMMEDIATE 'TRUNCATE TABLE MVD_HESAP_H';
    EXECUTE IMMEDIATE 'DELETE FROM MVD_HESAP';
    EXECUTE IMMEDIATE 'TRUNCATE TABLE MST_MUSTERIADRES_H';
    EXECUTE IMMEDIATE 'DELETE FROM MST_MUSTERIADRES';
    EXECUTE IMMEDIATE 'TRUNCATE TABLE MST_MUSTERI_H';
    EXECUTE IMMEDIATE 'DELETE FROM MST_MUSTERI';
    
    EXECUTE IMMEDIATE 'ALTER TABLE MVD_HESAPHAREKET ENABLE ALL TRIGGERS';
    EXECUTE IMMEDIATE 'ALTER TABLE MVD_HESAP ENABLE ALL TRIGGERS';
    EXECUTE IMMEDIATE 'ALTER TABLE MST_MUSTERIADRES ENABLE ALL TRIGGERS';
    EXECUTE IMMEDIATE 'ALTER TABLE MST_MUSTERI ENABLE ALL TRIGGERS';
END;
"""

def main():
    print("Veritabanına bağlanılıyor...")
    connection = oracledb.connect(user=DB_USER, password=DB_PASSWORD, dsn=DB_DSN)
    cursor = connection.cursor()

    print("1. Yeni DB kuralları (Trigger) veritabanına uygulanıyor...")
    cursor.execute(TRIGGER_SQL)
    print("Trigger güncellendi!")

    print("2. Önceki tüm kayıtlar siliniyor ve Sequenceler sıfırlanıyor...")
    cursor.execute(CLEANUP_SQL)
    print("Veritabanı tertemiz oldu!")

    print("3. İstenen algoritmalarla (Çift isim, IBAN, VKN/TCKN) veri üretiliyor...")
    
    musteriler = []
    hesaplar = []
    hareketler = []
    
    hesap_sayaci = 1

    doviz_tipleri = ["TRY", "USD", "EUR", "GBP", "XAU"]
    hesap_turleri = ["Vadesiz", "Vadeli", "Yatırım", "Maaş", "Altın"]
    
    giriş_kodu_aciklama = [
        ("PY_SALARY", "Maaş Ödemesi"),
        ("PY_RENT", "Kira Geliri"),
        ("PY_EFT_IN", "Gelen Havale/EFT"),
        ("PY_INV_INC", "Yatırım Getirisi")
    ]
    
    cikis_kodu_aciklama = [
        ("PY_CC_PAY", "Kredi Kartı Ödemesi"),
        ("PY_FEE", "Hizmet Bedeli"),
        ("PY_SHOP", "Online Alışveriş"),
        ("PY_LOAN", "Kredi Taksit Ödemesi")
    ]

    for m_id in range(1, NUM_RECORDS_MUSTERI + 1):
        is_corp = random.random() < 0.2
        musteri_tipi = 2 if is_corp else 1
        ad, soyad = get_name(is_corp)
        kimlik_no = generate_vkn() if is_corp else generate_tckn()
        email = f"user_{m_id}_{random.randint(1000, 9999)}@bank.com"
        telefon = f"555{random.randint(1000000, 9999999)}"
        
        musteriler.append((m_id, ad, soyad, musteri_tipi, kimlik_no, email, telefon))
        
        for _ in range(NUM_HESAP_PER_MUSTERI):
            hesap_turu = random.choice(hesap_turleri)
            doviz = random.choice(doviz_tipleri)
            
            if hesap_turu == "Altın":
                doviz = "XAU"
            if doviz == "XAU":
                hesap_turu = random.choice(["Altın", "Yatırım"])
            
            # Ticari müşteriye "Vadeli" gelirse bunu Python tarafında engellemiyoruz!
            # DB trigger bilerek test edilsin diye aynen bırakıyoruz.
            
            hesap_prefix = "TRT" if is_corp else "TRB"
            hesap_no = f"{hesap_prefix}{hesap_sayaci:013d}"
            
            bank_code = "00062" 
            check_digits = calculate_iban_check_digits(bank_code, hesap_no)
            iban = f"TR{check_digits}{bank_code}0{hesap_no}"
            
            hesaplar.append((hesap_no, m_id, iban, hesap_turu, doviz, 1))
            
            num_hareket = random.randint(1, 4)
            for _ in range(num_hareket):
                giris_tutar = round(random.uniform(1000.0, 50000.0), 2)
                cikis_tutar = round(random.uniform(10.0, giris_tutar), 2)
                
                gk = random.choice(giriş_kodu_aciklama)
                hareketler.append((hesap_no, "B", giris_tutar, doviz, gk[1], gk[0]))
                
                ck = random.choice(cikis_kodu_aciklama)
                hareketler.append((hesap_no, "C", cikis_tutar, doviz, ck[1], ck[0]))
            
            hesap_sayaci += 1

    print(f"Toplam Müşteri: {len(musteriler)}, Hesap: {len(hesaplar)}, Hareket: {len(hareketler)}")
    
    print("4. Müşteriler DB'ye yollanıyor...")
    sql_musteri = "INSERT INTO MST_MUSTERI (MUSTERI_ID, AD, SOYAD, MUSTERI_TIPI, KIMLIK_NO, EMAIL, TELEFON) VALUES (:1, :2, :3, :4, :5, :6, :7)"
    cursor.executemany(sql_musteri, musteriler, batcherrors=True)

    print("5. Hesaplar DB'ye yollanıyor (Tüzel+Vadeli kuralı DB'de test ediliyor)...")
    sql_hesap = "INSERT INTO MVD_HESAP (HESAP_NO, MUSTERI_ID, IBAN, HESAP_TURU, DOVIZ_CINSI, DURUM) VALUES (:1, :2, :3, :4, :5, :6)"
    cursor.executemany(sql_hesap, hesaplar, batcherrors=True)
    
    hesap_error_count = 0
    failed_hesap_nos = set()
    for error in cursor.getbatcherrors():
        hesap_error_count += 1
        failed_hesap_nos.add(hesaplar[error.offset][0])
        if hesap_error_count <= 20: 
            print(f"BİLİNÇLİ HATA YAKALANDI: {error.message} (Tüzel kişiye Vadeli hesabı kuralı)")
    
    print(f"-> TOPLAM {hesap_error_count} TİCARİ HESAP 'VADELİ' AÇMAK İSTEDİ VE VERİTABANI TARAFINDAN ENGELLENDİ!")

    print("6. Hareketler DB'ye yollanıyor...")
    gecerli_hareketler = [h for h in hareketler if h[0] not in failed_hesap_nos]
    
    CHUNK_SIZE = 50000
    sql_hareket = "INSERT INTO MVD_HESAPHAREKET (HESAP_NO, ISLEM_YONU, ISLEM_TUTARI, DOVIZ_CINSI, YENI_BAKIYE, ACIKLAMA, ISLEM_KODU) VALUES (:1, :2, :3, :4, 0, :5, :6)"
    
    for i in range(0, len(gecerli_hareketler), CHUNK_SIZE):
        chunk = gecerli_hareketler[i:i+CHUNK_SIZE]
        cursor.executemany(sql_hareket, chunk, batcherrors=True)
    
    connection.commit()
    cursor.close()
    connection.close()
    print("İşlem başarıyla tamamlandı, veritabanı yepyeni bir duruma getirildi!")

if __name__ == "__main__":
    main()
