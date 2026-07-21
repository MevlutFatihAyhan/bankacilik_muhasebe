import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HesapHareket } from '../models/hesap-hareket.model';

@Injectable({
    providedIn: 'root'
})
export class HesapHareketService {
    private apiUrl = 'http://localhost:5064/api/HesapHareket';

    constructor(private http: HttpClient) { }

    getAllHareketler(): Observable<HesapHareket[]> {
        return this.http.get<HesapHareket[]>(this.apiUrl);
    }

    getHareketler(hesapNo: string): Observable<HesapHareket[]> {
        return this.http.get<HesapHareket[]>(`${this.apiUrl}/hesap/${hesapNo}`); // usually filtered by account
    }

    getHareketById(islemId: number): Observable<HesapHareket> {
        return this.http.get<HesapHareket>(`${this.apiUrl}/${islemId}`);
    }

    addHareket(hareket: HesapHareket): Observable<any> {
        return this.http.post(this.apiUrl, hareket);
    }
}
