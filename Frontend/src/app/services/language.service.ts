import { Injectable, signal, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Language, TRANSLATIONS } from '../i18n/translations';

@Injectable({
  providedIn: 'root'
})
export class LanguageService {
  currentLang = signal<Language>('tr');

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {
    if (isPlatformBrowser(this.platformId)) {
      const savedLang = localStorage.getItem('app_lang') as Language;
      if (savedLang && (savedLang === 'tr' || savedLang === 'en')) {
        // 1. Kullanıcının daha önce manuel seçtiği dil varsa onu kullan
        this.currentLang.set(savedLang);
      } else {
        // 2. İlk defa giriyorsa tarayıcı / sistem dilini otomatik algıla
        const browserLang = (navigator.language || (navigator as any).userLanguage || '').toLowerCase();
        if (browserLang.startsWith('tr')) {
          this.currentLang.set('tr');
        } else {
          this.currentLang.set('en');
        }
      }
    }
  }

  setLanguage(lang: Language) {
    this.currentLang.set(lang);
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('app_lang', lang);
    }
  }

  t(key: string): string {
    const lang = this.currentLang();
    const dictionary = TRANSLATIONS[lang] || TRANSLATIONS['tr'];
    return dictionary[key] || TRANSLATIONS['tr'][key] || key;
  }
}
