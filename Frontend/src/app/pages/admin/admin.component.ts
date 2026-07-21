import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})
export class AdminComponent {
  isMusteriMenuOpen = true;
  isMusteriEkleMenuOpen = true;
  isHesapMenuOpen = true;
  isProfileMenuOpen = false;

  constructor(private router: Router) {}

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

  logout() {
    this.router.navigate(['/login']);
  }
}
