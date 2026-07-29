export interface HesapHareket {
    islemId: number;
    hesapNo: string;
    islemYonu: string;
    islemTutari: number;
    dovizCinsi: string;
    yeniBakiye: number;
    islemTarihi: string | Date;
    aciklama: string;
    islemKodu: string;
    referansNo: string;
    // Hesabın sahibi — yalnızca filtreli listede (PRC_HAREKET_LISTE) dolar
    musteriId?: number | null;
    musteriAdi?: string | null;
    musteriSoyadi?: string | null;
}

// Hareket listeleme filtresi — PKG_HESAP.PRC_HAREKET_LISTE parametreleriyle birebir eşleşir
export interface HesapHareketFiltre {
    searchTerm?: string | null;
    id?: string | null;             // Hesap No / İşlem ID / Hesap ID
    islemYonu?: string | null;      // 'B': para girişi, 'C': para çıkışı
    dovizCinsi?: string | null;     // TRY / USD / EUR / XAU
    baslangicTarihi?: string | null; // yyyy-MM-dd
    bitisTarihi?: string | null;     // yyyy-MM-dd
    minTutar?: number | null;
    maxTutar?: number | null;
    hesapNo?: string | null;
    musteriAdi?: string | null;
    musteriSoyadi?: string | null;
}
