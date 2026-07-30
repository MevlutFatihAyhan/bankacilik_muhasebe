import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, shareReplay, tap } from 'rxjs';
import { Musteri, MusteriFiltre } from '../models/musteri.model';
import { environment } from '../../environments/environments';

@Injectable({
    providedIn: 'root'
})
export class MusteriService {
    private apiUrl = environment.apiUrl + '/Musteri';
    private musteriCache$: Observable<Musteri[]> | null = null;

    constructor(private http: HttpClient) { }

    // API'den listeyi getir (GET) - Caching destekli
    getMusteriler(forceRefresh: boolean = false): Observable<Musteri[]> {
        if (!this.musteriCache$ || forceRefresh) {
            this.musteriCache$ = this.http.get<Musteri[]>(this.apiUrl).pipe(
                shareReplay(1)
            );
        }
        return this.musteriCache$;
    }

    // Özet Listesi (Sadece Id, Ad, Soyad) - Sınırsız
    getMusteriOzet(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/ozet`);
    }

    clearCache(): void {
        this.musteriCache$ = null;
    }

    // Filtreye bağlı listeleme — sorgu DB'de (PKG_MUSTERI.PRC_MUSTERI_LISTE) çalışır,
    // sadece "Uygula" butonuna basıldığında çağrılır. Cache kullanılmaz.
    getMusterilerByFilter(filtre: MusteriFiltre): Observable<Musteri[]> {
        let params = new HttpParams();

        if (filtre.searchTerm) params = params.set('searchTerm', filtre.searchTerm.trim());
        if (filtre.ad) params = params.set('ad', filtre.ad.trim());
        if (filtre.soyad) params = params.set('soyad', filtre.soyad.trim());
        if (filtre.musteriTipi != null) params = params.set('musteriTipi', filtre.musteriTipi);
        if (filtre.aktifMi != null) params = params.set('aktifMi', filtre.aktifMi);

        return this.http.get<Musteri[]>(`${this.apiUrl}/filtre`, { params });
    }

    getMusteriById(id: number): Observable<Musteri> {
        return this.http.get<Musteri>(`${this.apiUrl}/${id}`);
    }

    // API'ye yeni kayıt gönder (POST) - Cache temizler
    addMusteri(musteri: Musteri): Observable<any> {
        return this.http.post<any>(this.apiUrl, musteri).pipe(
            tap(() => this.clearCache())
        );
    }

    updateMusteri(musteri: Musteri): Observable<any> {
        return this.http.put<any>(this.apiUrl, musteri).pipe(
            tap(() => this.clearCache())
        );
    }

    deleteMusteri(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/${id}`).pipe(
            tap(() => this.clearCache())
        );
    }
}

