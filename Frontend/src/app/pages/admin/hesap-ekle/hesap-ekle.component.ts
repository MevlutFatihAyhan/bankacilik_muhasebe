import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../../pipes/translate.pipe';
import { HesapService } from '../../../services/hesap.service';
import { MusteriService } from '../../../services/musteri.service';
import { ToastService } from '../../../services/toast.service';
import { Router } from '@angular/router';
import { Musteri } from '../../../models/musteri.model';

@Component({
  selector: 'app-hesap-ekle',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './hesap-ekle.component.html',
  styleUrls: ['../tuzel-musteri-ekle/tuzel-musteri-ekle.component.css', './hesap-ekle.component.css']
})
export class HesapEkleComponent implements OnInit {
  musteriId: number = 0;
  hesapTuru: string = 'Vadesiz';
  dovizCinsi: string = 'TRY';

  // Seçili müşteri bilgileri
  secilenMusteri: Musteri | null = null;

  // Dialog state
  dialogAcik: boolean = false;
  dialogAramaAd: string = '';
  dialogAramaSoyad: string = '';
  dialogAramaId: string = '';
  dialogSonuclar: Musteri[] = [];
  dialogYukleniyor: boolean = false;
  dialogAramaYapildi: boolean = false;

  // Tüm müşteriler (cache)
  private tumMusteriler: Musteri[] = [];

  constructor(
    private hesapService: HesapService,
    private musteriService: MusteriService,
    private toastService: ToastService,
    private router: Router
  ) { }

  ngOnInit() {
    // Müşterileri önceden yükle
    this.musteriService.getMusteriler().subscribe({
      next: (data) => this.tumMusteriler = data,
      error: () => { }
    });
  }

  // --- Dialog Yönetimi ---
  dialogAc() {
    this.dialogAcik = true;
    this.dialogAramaAd = '';
    this.dialogAramaSoyad = '';
    this.dialogAramaId = '';
    this.dialogSonuclar = [];
    this.dialogAramaYapildi = false;
  }

  dialogKapat() {
    this.dialogAcik = false;
  }

  dialogAramaUygula() {
    this.dialogAramaYapildi = true;
    const adFil = this.dialogAramaAd.toLowerCase().trim();
    const soyadFil = this.dialogAramaSoyad.toLowerCase().trim();
    const idFil = this.dialogAramaId.trim();

    if (!adFil && !soyadFil && !idFil) {
      this.dialogSonuclar = this.tumMusteriler.slice(0, 50);
      return;
    }

    this.dialogSonuclar = this.tumMusteriler.filter(m => {
      const adEsles = adFil ? m.ad.toLowerCase().includes(adFil) : true;
      const soyadEsles = soyadFil ? m.soyad.toLowerCase().includes(soyadFil) : true;
      const idEsles = idFil ? String(m.musteriId).includes(idFil) : true;
      return adEsles && soyadEsles && idEsles;
    });
  }

  musteriSec(musteri: Musteri) {
    this.secilenMusteri = musteri;
    this.musteriId = musteri.musteriId;
    this.dialogKapat();
  }

  musteriSifirla() {
    this.secilenMusteri = null;
    this.musteriId = 0;
  }

  // --- Hesap Kaydet ---
  kaydet() {
    if (!this.musteriId || this.musteriId <= 0) {
      this.toastService.showWarning('Lütfen geçerli bir Müşteri ID girin!');
      return;
    }

    // Hesap no ve IBAN'ı backend üretir (IbanHelper → müşteri tipine göre TRB/TRT prefix)
    this.hesapService.addHesap({
      hesapNo: '',
      musteriId: this.musteriId,
      iban: '',
      hesapTuru: this.hesapTuru,
      dovizCinsi: this.dovizCinsi,
      bakiye: 0,
      durum: 1
    }).subscribe({
      next: () => {
        this.toastService.showSuccess('Hesap başarıyla açıldı!');
        this.router.navigate(['/admin/hesap-listeleri']);
      },
      error: (err) => {
        console.error(err);
        const errorMsg = err.error?.message || err.message || 'Bilinmeyen bir hata oluştu!';
        this.toastService.showError('Hata: ' + errorMsg);
      }
    });
  }
}
