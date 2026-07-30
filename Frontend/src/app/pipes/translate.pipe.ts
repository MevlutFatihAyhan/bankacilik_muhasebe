import { Pipe, PipeTransform } from '@angular/core';
import { LanguageService } from '../services/language.service';

@Pipe({
  name: 'translate',
  standalone: true,
  pure: false
})
export class TranslatePipe implements PipeTransform {
  private lastKey: string = '';
  private lastLang: string = '';
  private lastValue: string = '';

  constructor(private langService: LanguageService) {}

  transform(key: string): string {
    if (!key) return '';
    const currentLang = this.langService.currentLang();
    if (key === this.lastKey && currentLang === this.lastLang) {
      return this.lastValue;
    }
    this.lastKey = key;
    this.lastLang = currentLang;
    this.lastValue = this.langService.t(key);
    return this.lastValue;
  }
}
