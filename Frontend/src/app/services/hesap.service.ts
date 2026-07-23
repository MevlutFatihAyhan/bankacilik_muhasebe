import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay, tap } from 'rxjs';
import { Hesap } from '../models/hesap.model';
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
}

