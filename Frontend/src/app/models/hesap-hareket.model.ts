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
}
