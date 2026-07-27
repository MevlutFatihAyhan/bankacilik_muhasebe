import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { FilterPipe } from '../../../pipes/filter.pipe';
import { SortPipe } from '../../../pipes/sort.pipe';
import { HesapService } from '../../../services/hesap.service';
import { MusteriService } from '../../../services/musteri.service';
import { Hesap, HesapFiltre } from '../../../models/hesap.model';
import { Musteri } from '../../../models/musteri.model';
import { ToastService } from '../../../services/toast.service';

@Component({
  selector: 'app-hesap-listeleri',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, FilterPipe, SortPipe],
  templateUrl: './hesap-listeleri.component.html'
})
export class HesapListeleriComponent {
  searchTerm: string = '';
  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  currentPage: number = 1;
  pageSize: number = 10;
  Math = Math;

  // Advanced Filters (Sayfaya girince varsayılan olarak AÇIK)
  isFilterOpen: boolean = true;
  hasAppliedFilters: boolean = false; // Bilgiler ilk etapta direkt listelenmeyecek

  filterMusteriTipi: string = 'Tümü';
  filterHesapTuru: string = 'Tümü';
  filterDoviz: string = 'Tümü';
  filterDurum: string = 'Tümü';
  filterMinBakiye: number | null = null;
  filterMaxBakiye: number | null = null;

  hesaplar: Hesap[] = [];
  musterilerMap: Map<number, Musteri> = new Map();
  isLoading: boolean = false;

  constructor(
    private hesapService: HesapService,
    private musteriService: MusteriService,
    private toastService: ToastService
  ) { }

  // Sayfa açılışında hiçbir listeleme yapılmaz; veri yalnızca "Uygula" ile
  // seçilen kriterlere göre DB'den (PKG_HESAP.PRC_HESAP_LISTE) çekilir.
  private loadMusteriMap(): void {
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
  private buildFiltre(): HesapFiltre {
    return {
      searchTerm: this.searchTerm ? this.searchTerm.trim() : null,
      musteriTipi: this.filterMusteriTipi === 'Bireysel' ? 1
        : (this.filterMusteriTipi === 'Tüzel' ? 2 : null),
      hesapTuru: this.filterHesapTuru === 'Tümü' ? null : this.filterHesapTuru,
      dovizCinsi: this.filterDoviz === 'Tümü' ? null : this.filterDoviz,
      durum: this.filterDurum === 'Aktif' ? 1
        : (this.filterDurum === 'Pasif' ? 2 : (this.filterDurum === 'Kapalı' ? 3 : null)),
      minBakiye: this.filterMinBakiye,
      maxBakiye: this.filterMaxBakiye
    };
  }

  applyFilters() {
    this.currentPage = 1;
    this.isLoading = true;
    this.hesaplar = [];

    // Müşteri adlarını gösterebilmek için eşleştirme tablosunu bir kez yükle
    if (this.musterilerMap.size === 0) {
      this.loadMusteriMap();
    }

    this.hesapService.getHesaplarByFilter(this.buildFiltre()).subscribe({
      next: (data) => {
        this.hesaplar = data || [];
        this.hasAppliedFilters = true;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Hesaplar filtrelenirken hata oluştu:', err);
        this.hesaplar = [];
        this.hasAppliedFilters = true;
        this.isLoading = false;
      }
    });
  }

  resetFilters() {
    this.searchTerm = '';
    this.filterMusteriTipi = 'Tümü';
    this.filterHesapTuru = 'Tümü';
    this.filterDoviz = 'Tümü';
    this.filterDurum = 'Tümü';
    this.filterMinBakiye = null;
    this.filterMaxBakiye = null;
    this.hasAppliedFilters = false;
    this.hesaplar = [];
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
        this.toastService.showError('Hesap durumu güncellenirken hata oluştu.');
      }
    });
  }

  // Filtreleme DB tarafında yapıldığı için burada ek bir eleme yoktur;
  // "Uygula" öncesinde liste boş kalır.
  get filteredData(): Hesap[] {
    return this.hasAppliedFilters ? this.hesaplar : [];
  }
}
