import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LanguageService } from '../../services/language.service';
import { AuthService } from '../../services/auth.service';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, TranslatePipe],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  username: string = '';
  pass: string = '';
  errorMessage: string = '';

  constructor(
    public langService: LanguageService,
    private authService: AuthService,
    private router: Router
  ) {}

  setLanguage(lang: 'tr' | 'en') {
    this.langService.setLanguage(lang);
  }

  onLogin() {
    this.errorMessage = '';

    if (!this.username.trim() || !this.pass.trim()) {
      this.errorMessage = 'LOGIN.ERROR_REQUIRED';
      return;
    }

    this.authService.login(this.username, this.pass).subscribe({
      next: () => {
        this.router.navigate(['/admin']);
      },
      error: (err) => {
        console.error('Giriş hatası:', err);
        this.errorMessage = 'LOGIN.ERROR_INVALID';
      }
    });
  }
}
