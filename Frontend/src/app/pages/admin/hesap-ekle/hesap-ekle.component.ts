import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HesapService } from '../../../services/hesap.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-hesap-ekle',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './hesap-ekle.component.html',
  styleUrls: ['../tuzel-musteri-ekle/tuzel-musteri-ekle.component.css']
})
export class HesapEkleComponent {
  musteriId: number = 0;
  hesapTuru: string = 'Vadesiz';
  dovizCinsi: string = 'TL';

  constructor(
    private hesapService: HesapService,
    private router: Router
  ) { }

  kaydet() {
    this.hesapService.addHesap({
      hesapNo: 'ACC' + Math.floor(Math.random() * 1000000),
      musteriId: this.musteriId,
      iban: 'TR000000000000000000000000',
      hesapTuru: this.hesapTuru,
      dovizCinsi: this.dovizCinsi,
      bakiye: 0,
      durum: 1
    }).subscribe({
      next: () => {
        alert('Hesap başarıyla açıldı!');
        this.router.navigate(['/admin/hesap-listeleri']);
      },
      error: (err) => { 
        console.error(err);
        const errorMsg = err.error?.message || err.message || 'Bilinmeyen bir hata oluştu!';
        alert('Hata: ' + errorMsg); 
      }
    });
  }
}
