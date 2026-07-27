export interface Hesap {
    hesapNo: string;
    musteriId: number;
    iban: string;
    hesapTuru: string;
    dovizCinsi: string;
    bakiye: number;
    durum: number;
}

// Hesap listeleme filtresi — PKG_HESAP.PRC_HESAP_LISTE parametreleriyle birebir eşleşir
export interface HesapFiltre {
    searchTerm?: string | null;
    musteriTipi?: number | null;   // 1: Bireysel, 2: Tüzel
    hesapTuru?: string | null;     // Vadeli / Vadesiz
    dovizCinsi?: string | null;    // TRY / USD / EUR / XAU
    durum?: number | null;         // 1: Aktif, 2: Pasif, 3: Kapalı
    minBakiye?: number | null;
    maxBakiye?: number | null;
}
