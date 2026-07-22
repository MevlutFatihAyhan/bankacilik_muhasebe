import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environments';

export interface MusteriIstatistik {
    aktifMusteriSayisi: number;
    bireyselSayisi: number;
    tuzelSayisi: number;
    toplamMusteri: number;
}

export interface HesapIstatistik {
    dovizCinsi: string;
    toplamBakiye: number;
    hesapSayisi: number;
    vadesizSayisi: number;
    vadeliSayisi: number;
}

export interface HacimIstatistik {
    dovizCinsi: string;
    islemYonu: string;
    toplamHacim: number;
    islemSayisi: number;
}

export interface SonIslem {
    islemId: number;
    hesapNo: string;
    islemYonu: string;
    islemTutari: number;
    dovizCinsi: string;
    yeniBakiye: number;
    aciklama: string;
    islemTarihi: string;
}

export interface DashboardSummary {
    musteriIstatistikleri: MusteriIstatistik;
    hesapIstatistikleri: HesapIstatistik[];
    hacimIstatistikleri: HacimIstatistik[];
    sonIslemler: SonIslem[];
}

@Injectable({
    providedIn: 'root'
})
export class DashboardService {
    private apiUrl = environment.apiUrl + '/Dashboard';

    constructor(private http: HttpClient) { }

    getSummary(): Observable<DashboardSummary> {
        return this.http.get<DashboardSummary>(`${this.apiUrl}/summary`);
    }
}
