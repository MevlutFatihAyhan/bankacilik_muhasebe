import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { HesapHareketService } from '../../../services/hesap-hareket.service';
import { HesapHareket } from '../../../models/hesap-hareket.model';
import { TranslatePipe } from '../../../pipes/translate.pipe';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';

@Component({
  selector: 'app-islem-detayi',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslatePipe],
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
    const data = document.getElementById('receipt-content');
    if (data) {
      html2canvas(data, { scale: 2, useCORS: true }).then(canvas => {
        const imgWidth = 210; // A4 width in mm
        const pageHeight = 297; // A4 height in mm
        const imgHeight = (canvas.height * imgWidth) / canvas.width;
        
        const contentDataURL = canvas.toDataURL('image/png');
        const pdf = new jsPDF('p', 'mm', 'a4');
        const position = 10; // Top margin
        
        pdf.addImage(contentDataURL, 'PNG', 0, position, imgWidth, imgHeight);
        pdf.save(`VaqivBank_Dekont_${this.islemId}.pdf`);
      });
    }
  }
}
