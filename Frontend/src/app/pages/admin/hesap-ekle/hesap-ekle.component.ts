import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HesapService } from '../../../services/hesap.service';

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
  
  constructor(private hesapService: HesapService) {}

  kaydet() {
    this.hesapService.addHesap({
      hesapNo: 'ACC' + Math.floor(Math.random() * 1000000),
      musteriId: this.musteriId,
      iban: 'TR000000000000000000000000',
      hesapTuru: this.hesapTuru,
      dovizCinsi: this.dovizCinsi,
      bakiye: 0,
      durum: 'AKTIF'
    }).subscribe({
      next: () => { 
        alert('Hesap başarıyla açıldı!');
        this.musteriId = 0;
        this.hesapTuru = 'Vadesiz';
        this.dovizCinsi = 'TL';
      },
      error: () => { alert('Hata oluştu!'); }
    });
  }
}
