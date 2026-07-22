import { Injectable, signal, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  isLoggedIn = signal<boolean>(false);

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private router: Router
  ) {
    if (isPlatformBrowser(this.platformId)) {
      const token = localStorage.getItem('auth_token');
      if (token) {
        this.isLoggedIn.set(true);
      }
    }
  }

  login(username: string, pass: string): boolean {
    // Gelecekte burası: return this.http.post('/api/auth/login', { username, pass }) şeklinde API'ye bağlanacak
    // Şimdilik Mock Doğrulama Yapısı (admin / 123456 veya herhangi dolu bilgi)
    const validUser = (username.trim().toLowerCase() === 'admin' && pass.trim() === '123456') || (username.trim() !== '' && pass.trim() !== '');

    if (validUser) {
      if (isPlatformBrowser(this.platformId)) {
        localStorage.setItem('auth_token', 'mock-jwt-token-12345');
        localStorage.setItem('auth_user', username);
      }
      this.isLoggedIn.set(true);
      return true;
    }
    return false;
  }

  logout() {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('auth_token');
      localStorage.removeItem('auth_user');
    }
    this.isLoggedIn.set(false);
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    if (isPlatformBrowser(this.platformId)) {
      return !!localStorage.getItem('auth_token');
    }
    return this.isLoggedIn();
  }
}
