import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';

@Component({
  selector: 'app-hesap-detayi',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './hesap-detayi.component.html'
})
export class HesapDetayiComponent implements OnInit {
  hesapId: string | null = null;
  hesapDetay: any = null;

  constructor(private route: ActivatedRoute) {}

  ngOnInit() {
    this.hesapId = this.route.snapshot.paramMap.get('id');
    // Dummy veriler
    this.hesapDetay = {
        no: this.hesapId,
        ad: 'FATİH AYHAN',
        iban: 'TR980001000000000000000123',
        tur: 'Vadesiz',
        doviz: 'Dolar',
        bakiye: 25000.50,
        sube: 'Kadıköy Şubesi',
        acilisTarihi: '10.02.2023'
    };
  }
}
