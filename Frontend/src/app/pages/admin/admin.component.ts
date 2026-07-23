import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { LanguageService } from '../../services/language.service';
import { AuthService } from '../../services/auth.service';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslatePipe],
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})
export class AdminComponent {
  isMusteriMenuOpen = false;
  isMusteriEkleMenuOpen = false;
  isHesapMenuOpen = false;
  isProfileMenuOpen = false;
  isUserMenuOpen: boolean = false;


  constructor(
    private router: Router,
    public langService: LanguageService,
    private authService: AuthService
  ) { }

  toggleMusteriMenu() {
    this.isMusteriMenuOpen = !this.isMusteriMenuOpen;
  }

  toggleMusteriEkleMenu() {
    this.isMusteriEkleMenuOpen = !this.isMusteriEkleMenuOpen;
  }

  toggleHesapMenu() {
    this.isHesapMenuOpen = !this.isHesapMenuOpen;
  }

  toggleProfileMenu() {
    this.isProfileMenuOpen = !this.isProfileMenuOpen;
  }

  toggleUserMenu() {
    this.isUserMenuOpen = !this.isUserMenuOpen;
  }

  setLanguage(lang: 'tr' | 'en') {
    this.langService.setLanguage(lang);
  }

  logout() {
    this.authService.logout();
  }
}
