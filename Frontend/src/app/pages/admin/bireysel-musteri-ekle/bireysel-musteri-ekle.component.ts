import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MusteriService } from '../../../services/musteri.service';
import { AdresService } from '../../../services/adres.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-bireysel-musteri-ekle',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './bireysel-musteri-ekle.component.html',
  styleUrls: ['../tuzel-musteri-ekle/tuzel-musteri-ekle.component.css']
})
export class BireyselMusteriEkleComponent {
  ad: string = '';
  soyad: string = '';
  email: string = '';
  telefon: string = '';
  tckn: string = '';
  sehir: string = '';
  ilce: string = '';
  postaKodu: string = '';
  adres: string = '';

  constructor(
    private musteriService: MusteriService,
    private adresService: AdresService,
    private router: Router
  ) { }

  kaydet() {
    this.musteriService.addMusteri({
      musteriId: 0,
      ad: this.ad,
      soyad: this.soyad,
      email: this.email,
      telefon: this.telefon,
      aktifMi: 1,
      musteriTipi: 1, // Bireysel
      kimlikNo: this.tckn
    }).subscribe({
      next: (response) => {
        // Backend returns a simple text string on success.
        // We need to fetch the customer list to find the ID of the new customer by TCKN
        this.musteriService.getMusteriler().subscribe({
          next: (musteriler) => {
            const yeniMusteri = musteriler.find(m => m.kimlikNo === this.tckn);
            if (yeniMusteri && this.sehir) {
              this.adresService.addAdres({
                adresId: 0,
                musteriId: yeniMusteri.musteriId,
                adresBaslik: 'Ev Adresi',
                ulke: 'Türkiye',
                sehir: this.sehir,
                ilce: this.ilce,
                postaKodu: this.postaKodu,
                acikAdres: this.adres
              }).subscribe();
            }
            alert('Bireysel Müşteri eklendi!');
            this.router.navigate(['/admin/musteri-listesi']);
          }
        });
      },
      error: (err) => {
        console.error(err);
        const errorMsg = err.error?.message || err.message || 'Bilinmeyen bir hata oluştu!';
        alert('Hata: ' + errorMsg);
      }
    });
  }
}
