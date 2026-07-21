import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Hesap } from '../models/hesap.model';
import { environment } from '../../environments/environments';

@Injectable({
    providedIn: 'root'
})
export class HesapService {
    private apiUrl = environment.apiUrl + '/Hesap';

    constructor(private http: HttpClient) { }

    getAllHesaplar(): Observable<Hesap[]> {
        return this.http.get<Hesap[]>(this.apiUrl);
    }

    getHesaplar(musteriId: number): Observable<Hesap[]> {
        return this.http.get<Hesap[]>(`${this.apiUrl}/musteri/${musteriId}`);
    }

    getHesapById(no: string): Observable<Hesap> {
        return this.http.get<Hesap>(`${this.apiUrl}/${no}`);
    }

    addHesap(hesap: Hesap): Observable<any> {
        return this.http.post(this.apiUrl, hesap);
    }
}
