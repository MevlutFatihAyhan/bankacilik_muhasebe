import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MusteriService } from '../../../services/musteri.service';
import { AdresService } from '../../../services/adres.service';
import { Musteri } from '../../../models/musteri.model';
import { MusteriAdres } from '../../../models/musteri-adres.model';

@Component({
  selector: 'app-musteri-detayi',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './musteri-detayi.component.html',
  styleUrl: './musteri-detayi.component.css'
})
export class MusteriDetayiComponent implements OnInit {
  musteriId: number | null = null;
  musteriDetay: Musteri | null = null;
  adresler: MusteriAdres[] = [];

  // Yeni Adres Ekle Form Alanları
  yeniAdresFormAcik: boolean = false;
  yeniAdresBaslik: string = 'Ev Adresi';
  yeniSehir: string = '';
  yeniIlce: string = '';
  yeniPostaKodu: string = '';
  yeniAcikAdres: string = '';

  constructor(
    private route: ActivatedRoute,
    private musteriService: MusteriService,
    private adresService: AdresService
  ) {}

  ngOnInit() {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.musteriId = Number(idParam);
      this.musteriDetayYukle();
      this.adresleriYukle();
    }
  }

  musteriDetayYukle() {
    if (this.musteriId) {
      this.musteriService.getMusteriById(this.musteriId).subscribe({
        next: (data) => {
          this.musteriDetay = data;
        },
        error: (err) => {
          console.error('Müşteri detayı yüklenemedi', err);
        }
      });
    }
  }

  adresleriYukle() {
    if (this.musteriId) {
      this.adresService.getAdresler(this.musteriId).subscribe({
        next: (data) => {
          this.adresler = data || [];
        },
        error: (err) => {
          console.error('Adresler yüklenemedi', err);
        }
      });
    }
  }

  yeniAdresEkle() {
    if (!this.musteriId) return;

    if (!this.yeniSehir || !this.yeniAcikAdres) {
      alert('Lütfen şehir ve açık adres alanlarını doldurun.');
      return;
    }

    const yeniAdres: MusteriAdres = {
      adresId: 0,
      musteriId: this.musteriId,
      adresBaslik: this.yeniAdresBaslik || 'Diğer',
      ulke: 'Türkiye',
      sehir: this.yeniSehir,
      ilce: this.yeniIlce,
      postaKodu: this.yeniPostaKodu,
      acikAdres: this.yeniAcikAdres
    };

    this.adresService.addAdres(yeniAdres).subscribe({
      next: () => {
        alert('Yeni adres başarıyla eklendi!');
        this.yeniAdresFormSifirla();
        this.yeniAdresFormAcik = false;
        this.adresleriYukle();
      },
      error: (err) => {
        console.error(err);
        const errorMsg = err.error?.message || err.message || 'Adres eklenirken bir hata oluştu!';
        alert('Hata: ' + errorMsg);
      }
    });
  }

  adresSil(adresId: number) {
    if (confirm('Bu adresi silmek istediğinize emin misiniz?')) {
      this.adresService.deleteAdres(adresId).subscribe({
        next: () => {
          alert('Adres silindi.');
          this.adresleriYukle();
        },
        error: (err) => {
          console.error(err);
          alert('Adres silinirken bir hata oluştu.');
        }
      });
    }
  }

  yeniAdresFormSifirla() {
    this.yeniAdresBaslik = 'Ev Adresi';
    this.yeniSehir = '';
    this.yeniIlce = '';
    this.yeniPostaKodu = '';
    this.yeniAcikAdres = '';
  }
}
