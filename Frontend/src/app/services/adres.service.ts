import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MusteriAdres } from '../models/musteri-adres.model';

@Injectable({
    providedIn: 'root'
})
export class AdresService {
    private apiUrl = 'http://localhost:5064/api/Adres';

    constructor(private http: HttpClient) { }

    getAdresler(musteriId: number): Observable<MusteriAdres[]> {
        return this.http.get<MusteriAdres[]>(`${this.apiUrl}/musteri/${musteriId}`);
    }

    addAdres(adres: MusteriAdres): Observable<any> {
        return this.http.post(this.apiUrl, adres);
    }

    updateAdres(adres: MusteriAdres): Observable<any> {
        return this.http.put(this.apiUrl, adres);
    }

    deleteAdres(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/${id}`);
    }
}
