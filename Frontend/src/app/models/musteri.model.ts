export interface Musteri {
    musteriId: number;
    ad: string;
    soyad: string;
    email: string;
    telefon: string;
    aktifmi: number;
    MUSTERI_TIPI?: number;
    TCKN_VKN?: string;
    olusturmaTarihi?: string | Date;
    guncellenmeTarihi?: string | Date;
}
