import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, shareReplay, tap } from 'rxjs';
import { HesapHareket, HesapHareketFiltre } from '../models/hesap-hareket.model';
import { environment } from '../../environments/environments';

@Injectable({
    providedIn: 'root'
})
export class HesapHareketService {
    private apiUrl = environment.apiUrl + '/HesapHareket';
    private hareketlerCache$: Observable<HesapHareket[]> | null = null;

    constructor(private http: HttpClient) { }

    getAllHareketler(forceRefresh: boolean = false): Observable<HesapHareket[]> {
        if (!this.hareketlerCache$ || forceRefresh) {
            this.hareketlerCache$ = this.http.get<HesapHareket[]>(this.apiUrl).pipe(
                shareReplay(1)
            );
        }
        return this.hareketlerCache$;
    }

    clearCache(): void {
        this.hareketlerCache$ = null;
    }

    // Filtreye bağlı listeleme — sorgu DB'de (PKG_HESAP.PRC_HAREKET_LISTE) çalışır,
    // sadece "Uygula" butonuna basıldığında çağrılır. Cache kullanılmaz.
    getHareketlerByFilter(filtre: HesapHareketFiltre): Observable<HesapHareket[]> {
        let params = new HttpParams();

        if (filtre.searchTerm) params = params.set('searchTerm', filtre.searchTerm.trim());
        if (filtre.id) params = params.set('id', filtre.id.trim());
        if (filtre.islemYonu) params = params.set('islemYonu', filtre.islemYonu);
        if (filtre.dovizCinsi) params = params.set('dovizCinsi', filtre.dovizCinsi);
        if (filtre.baslangicTarihi) params = params.set('baslangicTarihi', filtre.baslangicTarihi);
        if (filtre.bitisTarihi) params = params.set('bitisTarihi', filtre.bitisTarihi);
        if (filtre.minTutar != null) params = params.set('minTutar', filtre.minTutar);
        if (filtre.maxTutar != null) params = params.set('maxTutar', filtre.maxTutar);
        if (filtre.hesapNo) params = params.set('hesapNo', filtre.hesapNo.trim());
        if (filtre.musteriAdi) params = params.set('musteriAdi', filtre.musteriAdi.trim());
        if (filtre.musteriSoyadi) params = params.set('musteriSoyadi', filtre.musteriSoyadi.trim());

        return this.http.get<HesapHareket[]>(`${this.apiUrl}/filtre`, { params });
    }

    getHareketler(hesapNo: string): Observable<HesapHareket[]> {
        return this.http.get<HesapHareket[]>(`${this.apiUrl}/hesap/${hesapNo}`); // usually filtered by account
    }

    getHareketById(islemId: number): Observable<HesapHareket> {
        return this.http.get<HesapHareket>(`${this.apiUrl}/${islemId}`);
    }

    addHareket(hareket: HesapHareket): Observable<any> {
        return this.http.post(this.apiUrl, hareket).pipe(
            tap(() => this.clearCache())
        );
    }
}

