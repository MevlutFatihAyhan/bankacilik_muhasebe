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
