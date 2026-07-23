import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay, tap } from 'rxjs';
import { Musteri } from '../models/musteri.model';
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

    clearCache(): void {
        this.musteriCache$ = null;
    }

    getMusteriById(id: number): Observable<Musteri> {
        return this.http.get<Musteri>(`${this.apiUrl}/${id}`);
    }

    // API'ye yeni kayıt gönder (POST) - Cache temizler
    addMusteri(musteri: Musteri): Observable<any> {
        return this.http.post(this.apiUrl, musteri, { responseType: 'text' as 'json' }).pipe(
            tap(() => this.clearCache())
        );
    }

    updateMusteri(musteri: Musteri): Observable<any> {
        return this.http.put(this.apiUrl, musteri, { responseType: 'text' as 'json' }).pipe(
            tap(() => this.clearCache())
        );
    }

    deleteMusteri(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/${id}`).pipe(
            tap(() => this.clearCache())
        );
    }
}

