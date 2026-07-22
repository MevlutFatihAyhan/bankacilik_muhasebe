import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Musteri } from '../models/musteri.model';
import { environment } from '../../environments/environments';

@Injectable({
    providedIn: 'root'
})
export class MusteriService {
    private apiUrl = environment.apiUrl + '/Musteri';

    constructor(private http: HttpClient) { }

    // API'den listeyi getir (GET)
    getMusteriler(): Observable<Musteri[]> {
        return this.http.get<Musteri[]>(this.apiUrl);
    }

    getMusteriById(id: number): Observable<Musteri> {
        return this.http.get<Musteri>(`${this.apiUrl}/${id}`);
    }

    // API'ye yeni kayıt gönder (POST)
    addMusteri(musteri: Musteri): Observable<any> {
        return this.http.post(this.apiUrl, musteri, { responseType: 'text' as 'json' });
    }

    updateMusteri(musteri: Musteri): Observable<any> {
        return this.http.put(this.apiUrl, musteri, { responseType: 'text' as 'json' });
    }


    deleteMusteri(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/${id}`);
    }
}
