import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { HesapHareketService } from '../../../services/hesap-hareket.service';
import { HesapHareket } from '../../../models/hesap-hareket.model';

@Component({
  selector: 'app-islem-detayi',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './islem-detayi.component.html',
  styleUrl: './islem-detayi.component.css'
})
export class IslemDetayiComponent implements OnInit {
  islemId: string | null = null;
  dekont: HesapHareket | null = null;

  constructor(
    private route: ActivatedRoute,
    private hesapHareketService: HesapHareketService
  ) {}

  ngOnInit() {
    this.islemId = this.route.snapshot.paramMap.get('islemId');
    if (this.islemId) {
      this.hesapHareketService.getHareketById(Number(this.islemId)).subscribe({
        next: (data) => {
          this.dekont = data;
        },
        error: (err) => {
          console.error('Dekont verisi alınamadı', err);
        }
      });
    }
  }
  
  yazdir() {
    window.print();
  }
}
