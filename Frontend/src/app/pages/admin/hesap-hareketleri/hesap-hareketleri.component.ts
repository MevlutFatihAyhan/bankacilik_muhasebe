import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { FilterPipe } from '../../../pipes/filter.pipe';
import { SortPipe } from '../../../pipes/sort.pipe';
import { TranslatePipe } from '../../../pipes/translate.pipe';
import { HesapHareketService } from '../../../services/hesap-hareket.service';
import { HesapHareket, HesapHareketFiltre } from '../../../models/hesap-hareket.model';

@Component({
  selector: 'app-hesap-hareketleri',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, FilterPipe, SortPipe, TranslatePipe],
  templateUrl: './hesap-hareketleri.component.html'
})
export class HesapHareketleriComponent {
  searchTerm: string = '';
  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';
  
  currentPage: number = 1;
  pageSize: number = 10;
  Math = Math;
  
  // Advanced Filters (Sayfaya girince varsayılan olarak AÇIK)
  isFilterOpen: boolean = true;
  hasAppliedFilters: boolean = false; // Bilgiler ilk etapta direkt listelenmeyecek

  filterId: string = '';
  filterYon: string = 'Tümü';
  filterDoviz: string = 'Tümü';
  filterStartDate: string = '';
  filterEndDate: string = '';
  filterMinTutar: number | null = null;
  filterMaxTutar: number | null = null;

  hareketler: HesapHareket[] = [];
  isLoading: boolean = false;

  // Sayfa açılışında hiçbir listeleme yapılmaz; veri yalnızca "Uygula" ile
  // seçilen kriterlere göre DB'den (PKG_HESAP.PRC_HAREKET_LISTE) çekilir.
  constructor(private hesapHareketService: HesapHareketService) {}

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

  // Ekrandaki seçimleri DB prosedürünün beklediği kodlara çevirir
  // (ISLEM_YONU → 'B': para girişi, 'C': para çıkışı)
  private buildFiltre(): HesapHareketFiltre {
    return {
      searchTerm: this.searchTerm ? this.searchTerm.trim() : null,
      id: this.filterId ? this.filterId.trim() : null,
      islemYonu: this.filterYon === 'Tümü' ? null : this.filterYon,
      dovizCinsi: this.filterDoviz === 'Tümü' ? null : this.filterDoviz,
      baslangicTarihi: this.filterStartDate || null,
      bitisTarihi: this.filterEndDate || null,
      minTutar: this.filterMinTutar,
      maxTutar: this.filterMaxTutar
    };
  }

  applyFilters() {
    this.currentPage = 1;
    this.isLoading = true;
    this.hareketler = [];

    this.hesapHareketService.getHareketlerByFilter(this.buildFiltre()).subscribe({
      next: (data) => {
        this.hareketler = data || [];
        this.hasAppliedFilters = true;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Hesap hareketleri filtrelenirken hata oluştu:', err);
        this.hareketler = [];
        this.hasAppliedFilters = true;
        this.isLoading = false;
      }
    });
  }

  resetFilters() {
    this.searchTerm = '';
    this.filterId = '';
    this.filterYon = 'Tümü';
    this.filterDoviz = 'Tümü';
    this.filterStartDate = '';
    this.filterEndDate = '';
    this.filterMinTutar = null;
    this.filterMaxTutar = null;
    this.hasAppliedFilters = false;
    this.hareketler = [];
    this.currentPage = 1;
  }

  get filteredData(): HesapHareket[] {
    if (!this.hasAppliedFilters) return [];

    return this.hareketler.filter(h => {
      if (this.filterId && this.filterId.trim() !== '') {
        const searchId = this.filterId.trim().toLowerCase();
        const matchesHesapNo = h.hesapNo ? h.hesapNo.toString().toLowerCase().includes(searchId) : false;
        const matchesIslemId = h.islemId ? h.islemId.toString().toLowerCase().includes(searchId) : false;
        if (!matchesHesapNo && !matchesIslemId) {
          return false;
        }
      }
      return true;
    });
  }
}
