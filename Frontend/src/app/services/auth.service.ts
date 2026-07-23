import { Injectable, signal, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environments';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl + '/Admin';
  isLoggedIn = signal<boolean>(false);

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private router: Router,
    private http: HttpClient
  ) {
    if (isPlatformBrowser(this.platformId)) {
      const user = localStorage.getItem('admin_user');
      if (user) {
        this.isLoggedIn.set(true);
      }
    }
  }

  login(username: string, pass: string): Observable<any> {
    const payload = {
      adminKullaniciAdi: username,
      adminSifre: pass
    };

    return this.http.post<any>(`${this.apiUrl}/login`, payload).pipe(
      tap((res) => {
        if (isPlatformBrowser(this.platformId)) {
          // Token kullanmadan sadece giriş yapan kullanıcı bilgisini saklıyoruz
          localStorage.setItem('admin_user', JSON.stringify({
            id: res.adminId || res.AdminId,
            kullaniciAdi: res.kullaniciAdi || res.KullaniciAdi || username
          }));
        }
        this.isLoggedIn.set(true);
      })
    );
  }

  logout() {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('admin_user');
    }
    this.isLoggedIn.set(false);
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    if (isPlatformBrowser(this.platformId)) {
      return !!localStorage.getItem('admin_user');
    }
    return this.isLoggedIn();
  }
}

