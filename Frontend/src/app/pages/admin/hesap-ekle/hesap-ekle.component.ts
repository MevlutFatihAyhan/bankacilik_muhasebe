import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HesapService } from '../../../services/hesap.service';
import { Router } from '@angular/router';
import { IbanUtil } from '../../../utils/iban.util';

@Component({
  selector: 'app-hesap-ekle',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './hesap-ekle.component.html',
  styleUrls: ['../tuzel-musteri-ekle/tuzel-musteri-ekle.component.css']
})
export class HesapEkleComponent implements OnInit {
  musteriId: number = 0;
  hesapTuru: string = 'Vadesiz';
  dovizCinsi: string = 'TRY';
  generatedIban: string = '';
  generatedHesapNo: string = '';

  constructor(
    private hesapService: HesapService,
    private router: Router
  ) { }

  ngOnInit() {
    this.yeniIbanUret();
  }

  yeniIbanUret() {
    this.generatedHesapNo = IbanUtil.generateAccountNo();
    this.generatedIban = IbanUtil.generateTrIban(this.generatedHesapNo);
  }

  kaydet() {
    if (!this.musteriId || this.musteriId <= 0) {
      alert('Lütfen geçerli bir Müşteri ID girin!');
      return;
    }

    if (!this.generatedHesapNo || !this.generatedIban) {
      this.yeniIbanUret();
    }

    this.hesapService.addHesap({
      hesapNo: this.generatedHesapNo,
      musteriId: this.musteriId,
      iban: this.generatedIban,
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
