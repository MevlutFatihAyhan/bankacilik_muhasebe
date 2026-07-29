export interface Hesap {
    hesapNo: string;
    musteriId: number;
    iban: string;
    hesapTuru: string;
    dovizCinsi: string;
    bakiye: number;
    durum: number;
}

// Para transferi isteği — POST /api/Hesap/transfer gövdesi
export interface ParaTransferiIstek {
    senderIban: string;
    receiverIban: string;
    amount: number;
    description: string;
}

// PKG_HESAP.PRC_PARA_TRANSFERI sonucu.
// islemKodu '0' başarılı; 100–108 iş kuralı ihlali, 500 veritabanı hatası.
export interface ParaTransferiSonuc {
    islemKodu: string;
    message: string;
    referansNo: string | null;
    gonderenIslemId: number | null;
    aliciIslemId: number | null;
    basarili: boolean;
}

// Hesap listeleme filtresi — PKG_HESAP.PRC_HESAP_LISTE parametreleriyle birebir eşleşir
export interface HesapFiltre {
    searchTerm?: string | null;
    id?: string | null;
    musteriAdi?: string | null;
    musteriSoyadi?: string | null;
    musteriTipi?: number | null;   // 1: Bireysel, 2: Tüzel
    hesapTuru?: string | null;     // Vadeli / Vadesiz
    dovizCinsi?: string | null;    // TRY / USD / EUR / XAU
    durum?: number | null;         // 1: Aktif, 2: Pasif, 3: Kapalı
    minBakiye?: number | null;
    maxBakiye?: number | null;
}
