import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { FilterPipe } from '../../../pipes/filter.pipe';
import { SortPipe } from '../../../pipes/sort.pipe';
import { TranslatePipe } from '../../../pipes/translate.pipe';
import { MusteriService } from '../../../services/musteri.service';
import { Musteri } from '../../../models/musteri.model';
import { ToastService } from '../../../services/toast.service';

@Component({
  selector: 'app-musteri-listesi',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, FilterPipe, SortPipe, TranslatePipe],
  templateUrl: './musteri-listesi.component.html'
})
export class MusteriListesiComponent implements OnInit {
  searchTerm: string = '';
  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  currentPage: number = 1;
  pageSize: number = 5;
  Math = Math;

  // Advanced Filters (Sayfaya girince varsayılan olarak AÇIK)
  isFilterOpen: boolean = true;
  hasAppliedFilters: boolean = false; // Bilgiler ilk etapta direkt listelenmeyecek

  filterMusteriAdi: string = '';
  filterMusteriSoyadi: string = '';
  filterMusteriTipi: string = 'Tümü';
  filterDurum: string = 'Tümü';

  musteriler: Musteri[] = [];
  isLoading: boolean = false;

  constructor(
    private musteriService: MusteriService,
    private toastService: ToastService
  ) { }

  ngOnInit(): void {
    this.loadMusteriler();
  }

  loadMusteriler(forceRefresh: boolean = false): void {
    this.isLoading = true;
    this.musteriService.getMusteriler(forceRefresh).subscribe({
      next: (data) => {
        if (data && data.length > 0) {
          this.musteriler = data;
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('API hatası, veriler çekilemedi.', err);
        this.isLoading = false;
      }
    });
  }

  refreshData(): void {
    this.loadMusteriler(true);
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

  toggleFilter() {
    this.isFilterOpen = !this.isFilterOpen;
  }

  applyFilters() {
    this.hasAppliedFilters = true;
    this.currentPage = 1;
  }

  resetFilters() {
    this.searchTerm = '';
    this.filterMusteriAdi = '';
    this.filterMusteriSoyadi = '';
    this.filterMusteriTipi = 'Tümü';
    this.filterDurum = 'Tümü';
    this.hasAppliedFilters = false;
    this.currentPage = 1;
  }

  get filteredData(): Musteri[] {
    if (!this.hasAppliedFilters) {
      return [];
    }

    return this.musteriler.filter(m => {
      // 1. Text Search (searchTerm)
      let matchesSearch = true;
      if (this.searchTerm) {
        const term = this.searchTerm.toLowerCase();
        matchesSearch = Object.values(m).some(val => String(val).toLowerCase().includes(term));
      }

      // 2. Müşteri Adı Filtresi (ad)
      let matchesAd = true;
      if (this.filterMusteriAdi && this.filterMusteriAdi.trim() !== '') {
        const searchAd = this.filterMusteriAdi.trim().toLowerCase();
        matchesAd = m.ad ? m.ad.toLowerCase().includes(searchAd) : false;
      }

      // 3. Müşteri Soyadı Filtresi (soyad)
      let matchesSoyad = true;
      if (this.filterMusteriSoyadi && this.filterMusteriSoyadi.trim() !== '') {
        const searchSoyad = this.filterMusteriSoyadi.trim().toLowerCase();
        matchesSoyad = m.soyad ? m.soyad.toLowerCase().includes(searchSoyad) : false;
      }

      // 4. Müşteri Tipi Filtresi (1: Bireysel, 2: Tüzel)
      let matchesTipi = true;
      if (this.filterMusteriTipi === 'Bireysel') matchesTipi = m.musteriTipi === 1;
      if (this.filterMusteriTipi === 'Tüzel') matchesTipi = m.musteriTipi === 2;

      // 5. Durum Filtresi (1: Aktif, 0 veya 2: Pasif)
      let matchesDurum = true;
      if (this.filterDurum === 'Aktif') matchesDurum = m.aktifMi === 1;
      if (this.filterDurum === 'Pasif') matchesDurum = m.aktifMi !== 1;

      return matchesSearch && matchesAd && matchesSoyad && matchesTipi && matchesDurum;
    });
  }

  showInfo(musteri: Musteri) {
    // Okunacak bilgi fazla olduğu için varsayılan süre yerine daha uzun bir süre veriliyor.
    const detay = [
      `ID: ${musteri.musteriId}`,
      `Ad / Unvan: ${musteri.ad} ${musteri.soyad}`,
      `TCKN/VKN: ${musteri.kimlikNo}`,
      `Email: ${musteri.email}`,
      `Telefon: ${musteri.telefon}`,
      `Durum: ${musteri.aktifMi === 1 ? 'Aktif' : 'Pasif'}`
    ].join('\n');

    this.toastService.show(detay, 'info', 'Müşteri Detayları', 8000);
  }
}
