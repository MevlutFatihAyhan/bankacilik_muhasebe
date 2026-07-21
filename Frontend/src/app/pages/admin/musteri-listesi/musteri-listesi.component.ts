import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { FilterPipe } from '../../../pipes/filter.pipe';
import { SortPipe } from '../../../pipes/sort.pipe';
import { MusteriService } from '../../../services/musteri.service';
import { Musteri } from '../../../models/musteri.model';

@Component({
  selector: 'app-musteri-listesi',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, FilterPipe, SortPipe],
  templateUrl: './musteri-listesi.component.html'
})
export class MusteriListesiComponent implements OnInit {
  searchTerm: string = '';
  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';
  
  currentPage: number = 1;
  pageSize: number = 5;
  Math = Math;

  // Advanced Filters
  isFilterOpen: boolean = false;
  filterMusteriTipi: string = 'Tümü';
  filterDurum: string = 'Tümü';

  musteriler: Musteri[] = [];

  constructor(private musteriService: MusteriService) {}

  ngOnInit(): void {
    this.musteriService.getMusteriler().subscribe({
      next: (data) => {
        if (data && data.length > 0) {
          this.musteriler = data;
        }
      },
      error: (err) => {
        console.error('API hatası, veriler çekilemedi.', err);
      }
    });
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
    this.filterDurum = 'Tümü';
    this.currentPage = 1;
  }

  get filteredData(): Musteri[] {
    return this.musteriler.filter(m => {
      // 1. Text Search (searchTerm)
      let matchesSearch = true;
      if (this.searchTerm) {
        const term = this.searchTerm.toLowerCase();
        matchesSearch = Object.values(m).some(val => String(val).toLowerCase().includes(term));
      }

      // 2. Müşteri Tipi Filtresi (1: Bireysel, 2: Tüzel)
      let matchesTipi = true;
      if (this.filterMusteriTipi === 'Bireysel') matchesTipi = m.MUSTERI_TIPI === 1;
      if (this.filterMusteriTipi === 'Tüzel') matchesTipi = m.MUSTERI_TIPI === 2;

      // 3. Durum Filtresi (1: Aktif, 0: Pasif)
      let matchesDurum = true;
      if (this.filterDurum === 'Aktif') matchesDurum = m.aktifmi === 1;
      if (this.filterDurum === 'Pasif') matchesDurum = m.aktifmi === 0;

      return matchesSearch && matchesTipi && matchesDurum;
    });
  }

  showInfo(musteri: Musteri) {
    alert(`MÜŞTERİ DETAYLARI\n\nID: ${musteri.musteriId}\nAd / Unvan: ${musteri.ad} ${musteri.soyad}\nTCKN/VKN: ${musteri.TCKN_VKN}\nEmail: ${musteri.email}\nTelefon: ${musteri.telefon}\nDurum: ${musteri.aktifmi === 1 ? 'Aktif' : 'Pasif'}`);
  }
}
