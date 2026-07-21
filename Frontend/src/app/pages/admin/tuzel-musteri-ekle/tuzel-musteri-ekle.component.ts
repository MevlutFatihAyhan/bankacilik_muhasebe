import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MusteriService } from '../../../services/musteri.service';
import { AdresService } from '../../../services/adres.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-tuzel-musteri-ekle',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tuzel-musteri-ekle.component.html',
  styleUrls: ['./tuzel-musteri-ekle.component.css']
})
export class TuzelMusteriEkleComponent {
  ad: string = '';
  email: string = '';
  vkn: string = '';
  sehir: string = '';
  ilce: string = '';
  postaKodu: string = '';
  adres: string = '';

  constructor(
    private musteriService: MusteriService,
    private adresService: AdresService,
    private router: Router
  ) {}

  kaydet() {
    this.musteriService.addMusteri({
      musteriId: 0,
      ad: this.ad,
      soyad: '',
      email: this.email,
      telefon: '',
      aktifmi: 1,
      MUSTERI_TIPI: 2, // Tüzel
      TCKN_VKN: this.vkn
    }).subscribe({
      next: (m) => {
        // Also save address
        this.adresService.addAdres({
          adresId: 0,
          musteriId: m.musteriId,
          adresBaslik: 'Firma Adresi',
          ulke: 'Türkiye',
          sehir: this.sehir,
          ilce: this.ilce,
          postaKodu: this.postaKodu,
          acikAdres: this.adres
        }).subscribe();
        alert('Tüzel Müşteri eklendi!');
        this.router.navigate(['/admin/musteri-listesi']);
      },
      error: () => alert('Hata!')
    });
  }
}
