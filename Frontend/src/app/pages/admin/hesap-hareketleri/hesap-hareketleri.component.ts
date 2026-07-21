import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { FilterPipe } from '../../../pipes/filter.pipe';
import { SortPipe } from '../../../pipes/sort.pipe';
import { HesapHareketService } from '../../../services/hesap-hareket.service';
import { HesapHareket } from '../../../models/hesap-hareket.model';

@Component({
  selector: 'app-hesap-hareketleri',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, FilterPipe, SortPipe],
  templateUrl: './hesap-hareketleri.component.html'
})
export class HesapHareketleriComponent implements OnInit {
  searchTerm: string = '';
  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';
  
  currentPage: number = 1;
  pageSize: number = 10;
  Math = Math;
  
  // Advanced Filters
  isFilterOpen: boolean = false;
  filterYon: string = 'Tümü';
  filterDoviz: string = 'Tümü';
  filterStartDate: string = '';
  filterEndDate: string = '';
  filterMinTutar: number | null = null;
  filterMaxTutar: number | null = null;

  hareketler: HesapHareket[] = [];

  constructor(private hesapHareketService: HesapHareketService) {}

  ngOnInit(): void {
    this.hesapHareketService.getAllHareketler().subscribe({
      next: (data) => {
        this.hareketler = data;
      },
      error: (err) => {
        console.error('Hesap hareketleri çekilemedi', err);
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
    this.currentPage = 1; // Sıralama değiştiğinde ilk sayfaya dön
  }

  onSearchChange() {
    this.currentPage = 1; // Arama değiştiğinde ilk sayfaya dön
  }

  toggleFilter() {
    this.isFilterOpen = !this.isFilterOpen;
  }

  resetFilters() {
    this.searchTerm = '';
    this.filterYon = 'Tümü';
    this.filterDoviz = 'Tümü';
    this.filterStartDate = '';
    this.filterEndDate = '';
    this.filterMinTutar = null;
    this.filterMaxTutar = null;
    this.currentPage = 1;
  }

  get filteredData(): HesapHareket[] {
    return this.hareketler.filter(h => {
      // 1. Text Search
      let matchesSearch = true;
      if (this.searchTerm) {
        const term = this.searchTerm.toLowerCase();
        matchesSearch = Object.values(h).some(val => String(val).toLowerCase().includes(term));
      }

      // 2. İşlem Yönü
      let matchesYon = true;
      if (this.filterYon === 'Geliş (Gelir)') matchesYon = h.islemYonu === 'B';
      if (this.filterYon === 'Gidiş (Gider)') matchesYon = h.islemYonu === 'C';

      // 3. Döviz
      let matchesDoviz = true;
      if (this.filterDoviz !== 'Tümü') matchesDoviz = h.dovizCinsi === this.filterDoviz;

      // 4. Tarih Aralığı
      let matchesDate = true;
      const islemDate = new Date(h.islemTarihi).getTime();
      if (this.filterStartDate) {
        const start = new Date(this.filterStartDate).getTime();
        if (islemDate < start) matchesDate = false;
      }
      if (this.filterEndDate) {
        // Son günün sonuna kadar dahil etmek için
        const end = new Date(this.filterEndDate).getTime() + 86400000;
        if (islemDate > end) matchesDate = false;
      }

      // 5. Tutar Aralığı
      let matchesTutar = true;
      if (this.filterMinTutar !== null && h.islemTutari < this.filterMinTutar) matchesTutar = false;
      if (this.filterMaxTutar !== null && h.islemTutari > this.filterMaxTutar) matchesTutar = false;

      return matchesSearch && matchesYon && matchesDoviz && matchesDate && matchesTutar;
    });
  }
}
