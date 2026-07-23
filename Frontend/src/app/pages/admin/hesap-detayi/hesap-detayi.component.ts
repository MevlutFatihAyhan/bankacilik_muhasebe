import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { HesapService } from '../../../services/hesap.service';
import { MusteriService } from '../../../services/musteri.service';
import { Hesap } from '../../../models/hesap.model';
import { Musteri } from '../../../models/musteri.model';

@Component({
  selector: 'app-hesap-detayi',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './hesap-detayi.component.html',
  styleUrl: './hesap-detayi.component.css'
})
export class HesapDetayiComponent implements OnInit {
  hesapNo: string | null = null;
  hesap: Hesap | null = null;
  musteri: Musteri | null = null;
  isLoading: boolean = true;
  errorMessage: string = '';

  constructor(
    private route: ActivatedRoute,
    private hesapService: HesapService,
    private musteriService: MusteriService
  ) {}

  ngOnInit() {
    this.hesapNo = this.route.snapshot.paramMap.get('id');
    if (this.hesapNo) {
      this.loadHesapDetayi(this.hesapNo);
    }
  }

  loadHesapDetayi(hesapNo: string) {
    this.isLoading = true;
    this.hesapService.getHesapById(hesapNo).subscribe({
      next: (data) => {
        this.hesap = data;
        if (data && data.musteriId) {
          this.musteriService.getMusteriById(data.musteriId).subscribe({
            next: (m) => {
              this.musteri = m;
              this.isLoading = false;
            },
            error: () => {
              this.isLoading = false;
            }
          });
        } else {
          this.isLoading = false;
        }
      },
      error: (err) => {
        console.error('Hesap detayı alınamadı:', err);
        this.errorMessage = 'Hesap detayları yüklenirken hata oluştu.';
        this.isLoading = false;
      }
    });
  }

  durumGuncelle(yeniDurum: number) {
    if (!this.hesap || this.hesap.durum === yeniDurum) return;
    this.hesapService.updateHesapDurum(this.hesap.hesapNo, yeniDurum).subscribe({
      next: () => {
        if (this.hesap) this.hesap.durum = yeniDurum;
      },
      error: (err) => {
        console.error('Durum güncellenemedi:', err);
        alert('Hesap durumu güncellenirken hata oluştu.');
      }
    });
  }
}

