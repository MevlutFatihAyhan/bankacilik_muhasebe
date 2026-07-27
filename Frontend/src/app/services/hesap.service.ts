import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, shareReplay, tap } from 'rxjs';
import { Hesap, HesapFiltre } from '../models/hesap.model';
import { environment } from '../../environments/environments';

@Injectable({
    providedIn: 'root'
})
export class HesapService {
    private apiUrl = environment.apiUrl + '/Hesap';
    private hesaplarCache$: Observable<Hesap[]> | null = null;

    constructor(private http: HttpClient) { }

    getAllHesaplar(forceRefresh: boolean = false): Observable<Hesap[]> {
        if (!this.hesaplarCache$ || forceRefresh) {
            this.hesaplarCache$ = this.http.get<Hesap[]>(this.apiUrl).pipe(
                shareReplay(1)
            );
        }
        return this.hesaplarCache$;
    }

    clearCache(): void {
        this.hesaplarCache$ = null;
    }

    // Filtreye bağlı listeleme — sorgu DB'de (PKG_HESAP.PRC_HESAP_LISTE) çalışır,
    // sadece "Uygula" butonuna basıldığında çağrılır. Cache kullanılmaz.
    getHesaplarByFilter(filtre: HesapFiltre): Observable<Hesap[]> {
        let params = new HttpParams();

        if (filtre.searchTerm) params = params.set('searchTerm', filtre.searchTerm.trim());
        if (filtre.musteriTipi != null) params = params.set('musteriTipi', filtre.musteriTipi);
        if (filtre.hesapTuru) params = params.set('hesapTuru', filtre.hesapTuru);
        if (filtre.dovizCinsi) params = params.set('dovizCinsi', filtre.dovizCinsi);
        if (filtre.durum != null) params = params.set('durum', filtre.durum);
        if (filtre.minBakiye != null) params = params.set('minBakiye', filtre.minBakiye);
        if (filtre.maxBakiye != null) params = params.set('maxBakiye', filtre.maxBakiye);

        return this.http.get<Hesap[]>(`${this.apiUrl}/filtre`, { params });
    }

    getHesaplar(musteriId: number): Observable<Hesap[]> {
        return this.http.get<Hesap[]>(`${this.apiUrl}/musteri/${musteriId}`);
    }

    getHesapById(no: string): Observable<Hesap> {
        return this.http.get<Hesap>(`${this.apiUrl}/${no}`);
    }

    addHesap(hesap: Hesap): Observable<any> {
        return this.http.post(this.apiUrl, hesap).pipe(
            tap(() => this.clearCache())
        );
    }

    updateHesapDurum(hesapNo: string, durum: number): Observable<any> {
        return this.http.put(`${this.apiUrl}/${hesapNo}/durum`, { durum }).pipe(
            tap(() => this.clearCache())
        );
    }

    paraTransferi(payload: { senderIban: string, receiverIban: string, amount: number, description: string }): Observable<any> {
        return this.http.post(`${this.apiUrl}/transfer`, payload).pipe(
            tap(() => this.clearCache())
        );
    }
}


