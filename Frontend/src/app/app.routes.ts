import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { AdminComponent } from './pages/admin/admin.component';

export const routes: Routes = [
    { path: 'login', component: LoginComponent },
    { 
      path: 'admin', 
      component: AdminComponent,
      children: [
        { path: 'anasayfa', loadComponent: () => import('./pages/admin/anasayfa/anasayfa.component').then(m => m.AnasayfaComponent) },
        { path: 'musteri-listesi', loadComponent: () => import('./pages/admin/musteri-listesi/musteri-listesi.component').then(m => m.MusteriListesiComponent) },
        { path: 'musteri-detayi/:id', loadComponent: () => import('./pages/admin/musteri-detayi/musteri-detayi.component').then(m => m.MusteriDetayiComponent) },
        { path: 'bireysel-musteri-ekle', loadComponent: () => import('./pages/admin/bireysel-musteri-ekle/bireysel-musteri-ekle.component').then(m => m.BireyselMusteriEkleComponent) },
        { path: 'tuzel-musteri-ekle', loadComponent: () => import('./pages/admin/tuzel-musteri-ekle/tuzel-musteri-ekle.component').then(m => m.TuzelMusteriEkleComponent) },
        { path: 'hesap-listeleri', loadComponent: () => import('./pages/admin/hesap-listeleri/hesap-listeleri.component').then(m => m.HesapListeleriComponent) },
        { path: 'hesap-hareketleri', loadComponent: () => import('./pages/admin/hesap-hareketleri/hesap-hareketleri.component').then(m => m.HesapHareketleriComponent) },
        { path: 'islem-detayi/:islemId', loadComponent: () => import('./pages/admin/islem-detayi/islem-detayi.component').then(m => m.IslemDetayiComponent) },
        { path: 'hesap-ekle', loadComponent: () => import('./pages/admin/hesap-ekle/hesap-ekle.component').then(m => m.HesapEkleComponent) },
        { path: 'hesaplar/:id', loadComponent: () => import('./pages/admin/hesap-detayi/hesap-detayi.component').then(m => m.HesapDetayiComponent) },
        { path: '', redirectTo: 'anasayfa', pathMatch: 'full' }
      ]
    },
    { path: '', redirectTo: 'login', pathMatch: 'full' }
];
