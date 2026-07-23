import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { FilterPipe } from '../../../pipes/filter.pipe';
import { SortPipe } from '../../../pipes/sort.pipe';
import { HesapService } from '../../../services/hesap.service';
import { MusteriService } from '../../../services/musteri.service';
import { Hesap } from '../../../models/hesap.model';
import { Musteri } from '../../../models/musteri.model';

@Component({
  selector: 'app-hesap-listeleri',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, FilterPipe, SortPipe],
  templateUrl: './hesap-listeleri.component.html'
})
export class HesapListeleriComponent implements OnInit {
  searchTerm: string = '';
  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  currentPage: number = 1;
  pageSize: number = 10;
  Math = Math;

  // Advanced Filters
  isFilterOpen: boolean = false;
  filterMusteriTipi: string = 'Tümü';
  filterHesapTuru: string = 'Tümü';
  filterDoviz: string = 'Tümü';
  filterDurum: string = 'Tümü';
  filterMinBakiye: number | null = null;
  filterMaxBakiye: number | null = null;

  hesaplar: Hesap[] = [];
  musterilerMap: Map<number, Musteri> = new Map();
  isLoading: boolean = true;

  constructor(
    private hesapService: HesapService,
    private musteriService: MusteriService
  ) { }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.hesapService.getAllHesaplar(true).subscribe({
      next: (data) => {
        this.hesaplar = data || [];
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Hesaplar yüklenirken hata oluştu:', err);
        this.isLoading = false;
      }
    });

    this.musteriService.getMusteriler().subscribe({
      next: (musteriler) => {
        this.musterilerMap.clear();
        (musteriler || []).forEach(m => {
          this.musterilerMap.set(m.musteriId, m);
        });
      },
      error: (err) => {
        console.warn('Müşteriler yüklenemedi:', err);
      }
    });
  }

  getMusteriAdi(musteriId: number): string {
    const musteri = this.musterilerMap.get(musteriId);
    if (!musteri) return `Müşteri #${musteriId}`;
    return musteri.soyad ? `${musteri.ad} ${musteri.soyad}` : musteri.ad;
  }

  getMusteriTipi(musteriId: number): number {
    const musteri = this.musterilerMap.get(musteriId);
    return (musteri && musteri.musteriTipi) ? musteri.musteriTipi : 0;
  }

  sortBy(column: string) {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    this.currentPage = 1;
  }

  onSearchChange() {
    this.currentPage = 1;
  }

  toggleFilter() {
    this.isFilterOpen = !this.isFilterOpen;
  }

  resetFilters() {
    this.searchTerm = '';
    this.filterMusteriTipi = 'Tümü';
    this.filterHesapTuru = 'Tümü';
    this.filterDoviz = 'Tümü';
    this.filterDurum = 'Tümü';
    this.filterMinBakiye = null;
    this.filterMaxBakiye = null;
    this.currentPage = 1;
  }

  durumGuncelle(hesap: Hesap, yeniDurum: number): void {
    if (hesap.durum === yeniDurum) return;
    this.hesapService.updateHesapDurum(hesap.hesapNo, yeniDurum).subscribe({
      next: () => {
        hesap.durum = yeniDurum;
      },
      error: (err) => {
        console.error('Hesap durumu güncellenemedi:', err);
        alert('Hesap durumu güncellenirken hata oluştu.');
      }
    });
  }

  get filteredData(): Hesap[] {
    return this.hesaplar.filter(h => {
      // 1. Text Search
      let matchesSearch = true;
      if (this.searchTerm) {
        const term = this.searchTerm.toLowerCase();
        const musteriAdi = this.getMusteriAdi(h.musteriId).toLowerCase();
        matchesSearch = (
          (h.hesapNo && h.hesapNo.toLowerCase().includes(term)) ||
          (h.iban && h.iban.toLowerCase().includes(term)) ||
          (h.hesapTuru && h.hesapTuru.toLowerCase().includes(term)) ||
          (h.dovizCinsi && h.dovizCinsi.toLowerCase().includes(term)) ||
          musteriAdi.includes(term) ||
          String(h.bakiye).includes(term)
        );
      }

      // 2. Müşteri Tipi Filtresi (1: Bireysel, 2: Tüzel)
      let matchesMusteri = true;
      if (this.filterMusteriTipi !== 'Tümü') {
        const tip = this.getMusteriTipi(h.musteriId);
        if (this.filterMusteriTipi === 'Bireysel') matchesMusteri = tip === 1;
        else if (this.filterMusteriTipi === 'Tüzel') matchesMusteri = tip === 2;
      }

      // 3. Hesap Türü Filtresi
      let matchesTur = true;
      if (this.filterHesapTuru !== 'Tümü') matchesTur = h.hesapTuru === this.filterHesapTuru;

      // 4. Döviz Filtresi
      let matchesDoviz = true;
      if (this.filterDoviz !== 'Tümü') matchesDoviz = h.dovizCinsi === this.filterDoviz;

      // 5. Durum Filtresi (1: Aktif, 2: Pasif, 3: Kapalı)
      let matchesDurum = true;
      if (this.filterDurum !== 'Tümü') {
        if (this.filterDurum === 'Aktif') matchesDurum = h.durum === 1;
        else if (this.filterDurum === 'Pasif') matchesDurum = h.durum === 2;
        else if (this.filterDurum === 'Kapalı') matchesDurum = h.durum === 3;
      }

      // 6. Bakiye Aralığı Filtresi
      let matchesBakiye = true;
      if (this.filterMinBakiye !== null && h.bakiye < this.filterMinBakiye) matchesBakiye = false;
      if (this.filterMaxBakiye !== null && h.bakiye > this.filterMaxBakiye) matchesBakiye = false;

      return matchesSearch && matchesMusteri && matchesTur && matchesDoviz && matchesDurum && matchesBakiye;
    });
  }
}
