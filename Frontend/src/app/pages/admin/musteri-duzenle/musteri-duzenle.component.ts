import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MusteriService } from '../../../services/musteri.service';
import { Musteri } from '../../../models/musteri.model';

@Component({
  selector: 'app-musteri-duzenle',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './musteri-duzenle.component.html',
  styleUrl: './musteri-duzenle.component.css'
})
export class MusteriDuzenleComponent implements OnInit {
  musteriId: number = 0;
  ad: string = '';
  soyad: string = '';
  email: string = '';
  telefon: string = '';
  kimlikNo: string = '';
  musteriTipi: number = 1;
  aktifMi: number = 1;
  loading: boolean = true;

  constructor(
    private route: ActivatedRoute,
    private musteriService: MusteriService,
    private router: Router
  ) {}

  ngOnInit() {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.musteriId = Number(idParam);
      this.musteriService.getMusteriById(this.musteriId).subscribe({
        next: (data: Musteri) => {
          this.ad = data.ad || '';
          this.soyad = data.soyad || '';
          this.email = data.email || '';
          this.telefon = data.telefon || '';
          this.kimlikNo = data.kimlikNo || '';
          this.musteriTipi = data.musteriTipi || 1;
          this.aktifMi = data.aktifMi ?? 1;
          this.loading = false;
        },
        error: (err) => {
          console.error(err);
          alert('Müşteri bilgileri yüklenemedi!');
          this.loading = false;
        }
      });
    }
  }

  guncelle() {
    const guncelMusteri: Musteri = {
      musteriId: this.musteriId,
      ad: this.ad,
      soyad: this.soyad,
      email: this.email,
      telefon: this.telefon,
      kimlikNo: this.kimlikNo,
      musteriTipi: this.musteriTipi,
      aktifMi: Number(this.aktifMi)
    };

    this.musteriService.updateMusteri(guncelMusteri).subscribe({
      next: () => {
        alert('Müşteri başarıyla güncellendi!');
        this.router.navigate(['/admin/musteri-detayi', this.musteriId]);
      },
      error: (err) => {
        console.error(err);
        const errorMsg = err.error?.message || err.message || 'Güncelleme sırasında bir hata oluştu!';
        alert('Hata: ' + errorMsg);
      }
    });
  }

  iptal() {
    this.router.navigate(['/admin/musteri-detayi', this.musteriId]);
  }
}
