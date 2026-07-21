import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { FilterPipe } from '../../../pipes/filter.pipe';
import { SortPipe } from '../../../pipes/sort.pipe';
import { HesapService } from '../../../services/hesap.service';
import { Hesap } from '../../../models/hesap.model';

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

  // Advanced Filters
  isFilterOpen: boolean = false;
  filterMusteriTipi: string = 'Tümü';
  filterHesapTuru: string = 'Tümü';
  filterDoviz: string = 'Tümü';

  hesaplar: Hesap[] = [];

  constructor(private hesapService: HesapService) {}

  ngOnInit(): void {
    this.hesapService.getAllHesaplar().subscribe({
      next: (data) => {
        this.hesaplar = data;
      },
      error: (err) => {
        console.error('Hesaplar alınamadı', err);
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
  }

  toggleFilter() {
    this.isFilterOpen = !this.isFilterOpen;
  }

  resetFilters() {
    this.searchTerm = '';
    this.filterMusteriTipi = 'Tümü';
    this.filterHesapTuru = 'Tümü';
    this.filterDoviz = 'Tümü';
  }

  get filteredData() {
    return this.hesaplar.filter(h => {
      // 1. Text Search
      let matchesSearch = true;
      if (this.searchTerm) {
        const term = this.searchTerm.toLowerCase();
        matchesSearch = Object.values(h).some(val => String(val).toLowerCase().includes(term));
      }

      // 2. Müşteri Tipi Filtresi (Şimdilik mock, API'den tam gelene kadar es geçebiliriz veya musteriId bazlı bir şey yapılabilir. Ancak filtrede Bireysel/Tüzel vardı)
      let matchesMusteri = true;
      // if (this.filterMusteriTipi !== 'Tümü') matchesMusteri = (api'den tip gelmediği için şimdilik atlanıyor);

      // 3. Hesap Türü Filtresi
      let matchesTur = true;
      if (this.filterHesapTuru !== 'Tümü') matchesTur = h.hesapTuru === this.filterHesapTuru;

      // 4. Döviz Filtresi
      let matchesDoviz = true;
      if (this.filterDoviz !== 'Tümü') matchesDoviz = h.dovizCinsi === this.filterDoviz;

      return matchesSearch && matchesMusteri && matchesTur && matchesDoviz;
    });
  }
}
