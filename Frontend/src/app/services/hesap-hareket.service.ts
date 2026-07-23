import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay, tap } from 'rxjs';
import { HesapHareket } from '../models/hesap-hareket.model';
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

