import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../../pipes/translate.pipe';
import { MusteriService } from '../../../services/musteri.service';
import { AdresService } from '../../../services/adres.service';
import { ToastService } from '../../../services/toast.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-tuzel-musteri-ekle',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './tuzel-musteri-ekle.component.html',
  styleUrls: ['./tuzel-musteri-ekle.component.css']
})

export class TuzelMusteriEkleComponent {
  ad: string = '';
  email: string = '';
  telefon: string = '';
  vkn: string = '';
  sehir: string = '';
  ilce: string = '';
  postaKodu: string = '';
  adres: string = '';

  constructor(
    private musteriService: MusteriService,
    private adresService: AdresService,
    private toastService: ToastService,
    private router: Router
  ) { }

  kaydet() {
    const temzTelefon = (this.telefon || '').replace(/\D/g, '');
    const temzKimlikNo = (this.vkn || '').replace(/\s+/g, '');

    this.musteriService.addMusteri({
      musteriId: 0,
      ad: (this.ad || '').trim(),
      soyad: '',
      email: (this.email || '').trim(),
      telefon: temzTelefon,
      aktifMi: 1,
      musteriTipi: 2, // Tüzel
      kimlikNo: temzKimlikNo
    }).subscribe({
      next: (response) => {
        this.musteriService.getMusteriler(true).subscribe({
          next: (musteriler) => {
            const yeniMusteri = musteriler.find(m => m.kimlikNo === temzKimlikNo);
            if (yeniMusteri && this.sehir) {
              this.adresService.addAdres({
                adresId: 0,
                musteriId: yeniMusteri.musteriId,
                adresBaslik: 'Firma Adresi',
                ulke: 'Türkiye',
                sehir: this.sehir,
                ilce: this.ilce,
                postaKodu: this.postaKodu,
                acikAdres: this.adres
              }).subscribe();
            }
            this.toastService.showSuccess('Tüzel Müşteri eklendi!');
            this.router.navigate(['/admin/musteri-listesi']);
          }
        });
      },
      error: (err) => {
        console.error(err);
        const errorMsg = err.error?.message || err.message || 'Bilinmeyen bir hata oluştu!';
        this.toastService.showError('Hata: ' + errorMsg);
      }
    });
  }
}
