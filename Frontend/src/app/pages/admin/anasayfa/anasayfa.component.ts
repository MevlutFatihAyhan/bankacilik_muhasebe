import { Component, AfterViewInit, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, CommonModule } from '@angular/common';
import Chart from 'chart.js/auto';

@Component({
  selector: 'app-anasayfa',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './anasayfa.component.html',
  styleUrl: './anasayfa.component.css'
})
export class AnasayfaComponent implements AfterViewInit, OnInit {
  chart: any;
  
  // Animasyon için değişkenler
  totalVolume: number = 0;
  activeUsers: number = 0;
  corpUsers: number = 0;
  indUsers: number = 0;
  goldAccs: number = 0;
  dollarAccs: number = 0;
  termAccs: number = 0;
  demandAccs: number = 0;

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {}

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.animateValue('totalVolume', 0, 255854551.88, 1500);
      this.animateValue('activeUsers', 0, 256, 1500);
      this.animateValue('corpUsers', 0, 78, 1500);
      this.animateValue('indUsers', 0, 178, 1500);
      this.animateValue('goldAccs', 0, 68, 1500);
      this.animateValue('dollarAccs', 0, 101, 1500);
      this.animateValue('termAccs', 0, 88, 1500);
      this.animateValue('demandAccs', 0, 168, 1500);
    }
  }

  animateValue(propName: keyof AnasayfaComponent, start: number, end: number, duration: number) {
    let startTimestamp: number | null = null;
    const step = (timestamp: number) => {
      if (!startTimestamp) startTimestamp = timestamp;
      const progress = Math.min((timestamp - startTimestamp) / duration, 1);
      
      // Easing out (yavaşlayarak bitme efekti)
      const easeOut = 1 - Math.pow(1 - progress, 3);
      
      (this as any)[propName] = start + (end - start) * easeOut;
      
      if (progress < 1) {
        window.requestAnimationFrame(step);
      } else {
        (this as any)[propName] = end; // Ensure exact value at the end
      }
    };
    window.requestAnimationFrame(step);
  }

  ngAfterViewInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.createChart();
    }
  }

  createChart() {
    const ctx = document.getElementById('volumeChart') as HTMLCanvasElement;
    if (!ctx) return;

    this.chart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: ['1. Hafta', '2. Hafta', '3. Hafta', '4. Hafta'],
        datasets: [{
          label: 'Haftalık Hacim (Milyon ₺)',
          data: [45, 60, 55, 95],
          borderColor: '#DA0037', // Kiremit Kırmızısı (Ana Banka Rengi)
          borderWidth: 2,
          tension: 0.1,
          fill: false,
          pointBackgroundColor: '#DA0037',
          pointRadius: 3,
          pointHoverRadius: 5
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            backgroundColor: 'rgba(0,0,0,0.8)',
            padding: 12,
            titleFont: { size: 14, family: "'Elms Sans', sans-serif" },
            bodyFont: { size: 13, family: "'Elms Sans', sans-serif" },
            displayColors: false,
            callbacks: {
              label: function(context) {
                return context.parsed.y + ' Milyon ₺';
              }
            }
          }
        },
        scales: {
          y: {
            beginAtZero: true,
            grid: {
              color: '#f0f0f0'
            },
            ticks: {
              font: { family: "'Elms Sans', sans-serif" },
              callback: function(value) {
                return value + 'M';
              }
            }
          },
          x: {
            grid: {
              display: false
            },
            ticks: {
              font: { family: "'Elms Sans', sans-serif" }
            }
          }
        }
      }
    });
  }
}
