import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { FilterPipe } from '../../../pipes/filter.pipe';
import { SortPipe } from '../../../pipes/sort.pipe';
import { TranslatePipe } from '../../../pipes/translate.pipe';
import { MusteriService } from '../../../services/musteri.service';
import { Musteri, MusteriFiltre } from '../../../models/musteri.model';
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

  // Sayfa açılışında hiçbir listeleme yapılmaz; veri yalnızca "Uygula" ile
  // seçilen kriterlere göre DB'den (PKG_MUSTERI.PRC_MUSTERI_LISTE) çekilir.
  ngOnInit(): void { }

  // Filtre uygulanmışsa aynı kriterlerle DB'den yeniden çeker
  refreshData(): void {
    if (this.hasAppliedFilters) {
      this.applyFilters();
    }
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

  // Ekrandaki seçimleri DB prosedürünün beklediği kodlara çevirir
  // (MUSTERI_TIPI → 1: Bireysel, 2: Tüzel; AKTIF_MI → 1: Aktif, 2: Pasif)
  private buildFiltre(): MusteriFiltre {
    return {
      searchTerm: this.searchTerm ? this.searchTerm.trim() : null,
      ad: this.filterMusteriAdi ? this.filterMusteriAdi.trim() : null,
      soyad: this.filterMusteriSoyadi ? this.filterMusteriSoyadi.trim() : null,
      musteriTipi: this.filterMusteriTipi === 'Bireysel' ? 1
        : (this.filterMusteriTipi === 'Tüzel' ? 2 : null),
      aktifMi: this.filterDurum === 'Aktif' ? 1
        : (this.filterDurum === 'Pasif' ? 2 : null)
    };
  }

  applyFilters() {
    this.currentPage = 1;
    this.isLoading = true;
    this.musteriler = [];

    this.musteriService.getMusterilerByFilter(this.buildFiltre()).subscribe({
      next: (data) => {
        this.musteriler = data || [];
        this.hasAppliedFilters = true;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Müşteriler filtrelenirken hata oluştu:', err);
        this.musteriler = [];
        this.hasAppliedFilters = true;
        this.isLoading = false;
      }
    });
  }

  resetFilters() {
    this.searchTerm = '';
    this.filterMusteriAdi = '';
    this.filterMusteriSoyadi = '';
    this.filterMusteriTipi = 'Tümü';
    this.filterDurum = 'Tümü';
    this.hasAppliedFilters = false;
    this.musteriler = [];
    this.currentPage = 1;
  }

  // Filtreleme DB tarafında yapıldığı için burada ek bir eleme yoktur;
  // "Uygula" öncesinde liste boş kalır.
  get filteredData(): Musteri[] {
    return this.hasAppliedFilters ? this.musteriler : [];
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
