export interface Musteri {
    musteriId: number;
    ad: string;
    soyad: string;
    email: string;
    telefon: string;
    aktifMi: number;
    musteriTipi?: number;
    kimlikNo?: string;
    olusturmaTarihi?: string | Date;
    guncellemeTarihi?: string | Date;
}

// Müşteri listeleme filtresi — PKG_MUSTERI.PRC_MUSTERI_LISTE parametreleriyle birebir eşleşir
export interface MusteriFiltre {
    searchTerm?: string | null;
    ad?: string | null;             // Tüzel müşteride unvan
    soyad?: string | null;
    musteriTipi?: number | null;    // 1: Bireysel, 2: Tüzel
    aktifMi?: number | null;        // 1: Aktif, 0: Pasif
}
