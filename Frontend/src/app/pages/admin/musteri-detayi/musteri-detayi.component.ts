import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MusteriService } from '../../../services/musteri.service';
import { Musteri } from '../../../models/musteri.model';

@Component({
  selector: 'app-musteri-detayi',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './musteri-detayi.component.html',
  styleUrl: './musteri-detayi.component.css'
})
export class MusteriDetayiComponent implements OnInit {
  musteriId: string | null = null;
  musteriDetay: Musteri | null = null;

  constructor(
    private route: ActivatedRoute,
    private musteriService: MusteriService
  ) {}

  ngOnInit() {
    this.musteriId = this.route.snapshot.paramMap.get('id');
    if (this.musteriId) {
      this.musteriService.getMusteriById(Number(this.musteriId)).subscribe({
        next: (data) => {
          this.musteriDetay = data;
        },
        error: (err) => {
          console.error('Müşteri detayı yüklenemedi', err);
        }
      });
    }
  }
}
