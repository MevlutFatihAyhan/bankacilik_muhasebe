import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MusteriService } from '../../../services/musteri.service';
import { AdresService } from '../../../services/adres.service';

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
  tckn: string = '';
  sehir: string = '';
  ilce: string = '';
  postaKodu: string = '';
  adres: string = '';

  constructor(
    private musteriService: MusteriService,
    private adresService: AdresService
  ) {}

  kaydet() {
    this.musteriService.addMusteri({
      musteriId: 0,
      ad: this.ad,
      soyad: this.soyad,
      email: this.email,
      telefon: '',
      aktifmi: 1,
      MUSTERI_TIPI: 1, // Bireysel
      TCKN_VKN: this.tckn
    }).subscribe({
      next: (m) => {
        // Also save address
        this.adresService.addAdres({
          adresId: 0,
          musteriId: m.musteriId,
          adresBaslik: 'Ev Adresi',
          ulke: 'Türkiye',
          sehir: this.sehir,
          ilce: this.ilce,
          postaKodu: this.postaKodu,
          acikAdres: this.adres
        }).subscribe();
        alert('Bireysel Müşteri eklendi!');
        // clear form
        this.ad = ''; this.soyad = ''; this.email = ''; this.tckn = ''; this.sehir = ''; this.ilce = ''; this.postaKodu = ''; this.adres = '';
      },
      error: () => alert('Hata!')
    });
  }
}
