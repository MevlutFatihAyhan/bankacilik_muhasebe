import { Component, AfterViewInit, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import Chart from 'chart.js/auto';
import { DashboardService, DashboardSummary } from '../../../services/dashboard.service';
import { LanguageService } from '../../../services/language.service';
import { TranslatePipe } from '../../../pipes/translate.pipe';

@Component({
  selector: 'app-anasayfa',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
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

  summaryData: DashboardSummary | null = null;
  chartValues: number[] = [0, 0, 0, 0];

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    public router: Router,
    private dashboardService: DashboardService,
    public langService: LanguageService
  ) {}

  ngOnInit() {
    this.dashboardDataYukle();
  }

  dashboardDataYukle() {
    this.dashboardService.getSummary().subscribe({
      next: (data) => {
        this.summaryData = data;

        const musteri = data.musteriIstatistikleri;
        const targetActiveUsers = musteri?.aktifMusteriSayisi || 0;
        const targetCorpUsers = musteri?.tuzelSayisi || 0;
        const targetIndUsers = musteri?.bireyselSayisi || 0;

        const hesaplar = data.hesapIstatistikleri || [];
        const targetGoldAccs = hesaplar.find(h => h.dovizCinsi === 'ALTIN' || h.dovizCinsi === 'Altin')?.hesapSayisi || 0;
        const targetDollarAccs = hesaplar.find(h => h.dovizCinsi === 'USD' || h.dovizCinsi === 'Dolar')?.hesapSayisi || 0;
        
        let targetTermAccs = 0;
        let targetDemandAccs = 0;
        hesaplar.forEach(h => {
          targetTermAccs += h.vadeliSayisi || 0;
          targetDemandAccs += h.vadesizSayisi || 0;
        });

        const hacimler = data.hacimIstatistikleri || [];
        let targetTotalVolume = 0;
        if (hacimler.length > 0) {
          hacimler.forEach(h => {
            targetTotalVolume += h.toplamHacim || 0;
          });
        } else if (data.sonIslemler && data.sonIslemler.length > 0) {
          data.sonIslemler.forEach(s => {
            targetTotalVolume += s.islemTutari || 0;
          });
        }

        if (targetTotalVolume > 0) {
          this.chartValues = [
            Math.round(targetTotalVolume * 0.15),
            Math.round(targetTotalVolume * 0.35),
            Math.round(targetTotalVolume * 0.65),
            Math.round(targetTotalVolume)
          ];
        } else {
          this.chartValues = [0, 0, 0, 0];
        }


        if (isPlatformBrowser(this.platformId)) {
          this.animateValue('totalVolume', 0, targetTotalVolume, 1200);
          this.animateValue('activeUsers', 0, targetActiveUsers, 1200);
          this.animateValue('corpUsers', 0, targetCorpUsers, 1200);
          this.animateValue('indUsers', 0, targetIndUsers, 1200);
          this.animateValue('goldAccs', 0, targetGoldAccs, 1200);
          this.animateValue('dollarAccs', 0, targetDollarAccs, 1200);
          this.animateValue('termAccs', 0, targetTermAccs, 1200);
          this.animateValue('demandAccs', 0, targetDemandAccs, 1200);

          this.createChart(this.chartValues);
        }
      },
      error: (err) => {
        console.error('Dashboard verisi çekilemedi:', err);
        if (isPlatformBrowser(this.platformId)) {
          this.createChart([0, 0, 0, 0]);
        }
      }
    });
  }

  animateValue(propName: keyof AnasayfaComponent, start: number, end: number, duration: number) {
    let startTimestamp: number | null = null;
    const step = (timestamp: number) => {
      if (!startTimestamp) startTimestamp = timestamp;
      const progress = Math.min((timestamp - startTimestamp) / duration, 1);
      
      const easeOut = 1 - Math.pow(1 - progress, 3);
      
      (this as any)[propName] = start + (end - start) * easeOut;
      
      if (progress < 1) {
        window.requestAnimationFrame(step);
      } else {
        (this as any)[propName] = end;
      }
    };
    window.requestAnimationFrame(step);
  }

  ngAfterViewInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.createChart(this.chartValues);
    }
  }

  createChart(chartData: number[] = [0, 0, 0, 0]) {
    const ctx = document.getElementById('volumeChart') as HTMLCanvasElement;
    if (!ctx) return;

    if (this.chart) {
      this.chart.destroy();
    }

    const w1 = this.langService.t('DASHBOARD.WEEK_1');
    const w2 = this.langService.t('DASHBOARD.WEEK_2');
    const w3 = this.langService.t('DASHBOARD.WEEK_3');
    const w4 = this.langService.t('DASHBOARD.WEEK_4');
    const volumeLabel = this.langService.t('DASHBOARD.WEEKLY_VOLUME');

    this.chart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: [w1, w2, w3, w4],
        datasets: [{
          label: volumeLabel,
          data: chartData,
          borderColor: '#DA0037',
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
                const val = context.parsed?.y ?? 0;
                return val.toLocaleString('tr-TR') + ' ₺';
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
              precision: 0,
              font: { family: "'Elms Sans', sans-serif" },
              callback: function(value) {
                return Number(value).toLocaleString('tr-TR') + ' ₺';
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
