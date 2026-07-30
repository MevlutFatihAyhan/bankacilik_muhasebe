import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '../../../pipes/translate.pipe';
import { HesapService } from '../../../services/hesap.service';
import { MusteriService } from '../../../services/musteri.service';
import { ToastService } from '../../../services/toast.service';
import { Hesap } from '../../../models/hesap.model';
import { Musteri } from '../../../models/musteri.model';

@Component({
    selector: 'app-hesap-transferi',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule, TranslatePipe],
    templateUrl: './hesap-transferi.component.html',
    styleUrls: ['../hesap-ekle/hesap-ekle.component.css', './hesap-transferi.component.css']
})
export class HesapTransferiComponent implements OnInit {
    transferForm: FormGroup;
    isTransferSuccessful: boolean = false;

    // Transferden dönen gerçek hareket ID'leri
    senderTxId: number | null = null;
    receiverTxId: number | null = null;
    referansNo: string | null = null;

    // Dialog state
    dialogAcik: boolean = false;
    secilenDialogTuru: 'sender' | 'receiver' = 'sender';
    
    dialogAsama: 1 | 2 = 1;
    dialogAramaMusteriId: string = '';
    dialogAramaMusteriAd: string = '';
    dialogAramaMusteriSoyad: string = '';
    
    dialogSecilenMusteri: Musteri | null = null;
    dialogMusteriSonuclar: Musteri[] = [];
    dialogHesapSonuclar: Hesap[] = [];
    dialogAramaYapildi: boolean = false;

    // Seçili hesap kartları için
    secilenSenderHesap: Hesap | null = null;
    secilenReceiverHesap: Hesap | null = null;

    private tumHesaplar: Hesap[] = [];
    private tumMusteriler: Musteri[] = [];

    constructor(
        private fb: FormBuilder,
        private hesapService: HesapService,
        private musteriService: MusteriService,
        private toastService: ToastService
    ) {
        this.transferForm = this.fb.group({
            senderIban: ['', [Validators.required, Validators.minLength(26), Validators.maxLength(26)]],
            receiverIban: ['', [Validators.required, Validators.minLength(26), Validators.maxLength(26)]],
            description: ['', Validators.required],
            amount: ['', [Validators.required, Validators.min(0.01)]]
        });
    }

    ngOnInit() {
        this.hesapService.getAllHesaplar().subscribe({
            next: (data) => this.tumHesaplar = data,
            error: () => {}
        });

        this.musteriService.getMusteriler().subscribe({
            next: (data) => this.tumMusteriler = data,
            error: () => {}
        });
    }

    // --- Dialog Yönetimi ---
    dialogAc(tur: 'sender' | 'receiver') {
        this.secilenDialogTuru = tur;
        this.dialogAcik = true;
        this.dialogAsama = 1;
        this.dialogSecilenMusteri = null;
        
        this.dialogAramaMusteriId = '';
        this.dialogAramaMusteriAd = '';
        this.dialogAramaMusteriSoyad = '';
        
        this.dialogMusteriSonuclar = [];
        this.dialogHesapSonuclar = [];
        this.dialogAramaYapildi = false;
        
        // Müşteri listesini hemen yükle
        this.dialogMusteriAra();
    }

    dialogKapat() {
        this.dialogAcik = false;
    }

    dialogMusteriAra() {
        this.dialogAramaYapildi = true;
        const sId = this.dialogAramaMusteriId ? this.dialogAramaMusteriId.toLocaleLowerCase('tr-TR') : '';
        const sAd = this.dialogAramaMusteriAd ? this.dialogAramaMusteriAd.toLocaleLowerCase('tr-TR') : '';
        const sSoyad = this.dialogAramaMusteriSoyad ? this.dialogAramaMusteriSoyad.toLocaleLowerCase('tr-TR') : '';

        this.dialogMusteriSonuclar = this.tumMusteriler.filter(m => {
            const mId = String(m.musteriId || (m as any).musteriID || (m as any).MusteriID).toLocaleLowerCase('tr-TR');
            const mAd = m.ad ? m.ad.toLocaleLowerCase('tr-TR') : '';
            const mSoyad = m.soyad ? m.soyad.toLocaleLowerCase('tr-TR') : '';

            let uyar = true;
            if (sId && !mId.includes(sId)) uyar = false;
            if (sAd && !mAd.includes(sAd)) uyar = false;
            if (sSoyad && !mSoyad.includes(sSoyad)) uyar = false;
            return uyar;
        });
    }

    dialogMusteriSec(musteri: Musteri) {
        this.dialogSecilenMusteri = musteri;
        this.dialogAsama = 2;
        this.dialogHesapSonuclar = [];
        const musteriId = musteri.musteriId || (musteri as any).musteriID || (musteri as any).MusteriID;
        
        this.hesapService.getHesaplar(musteriId).subscribe({
            next: (data) => {
                this.dialogHesapSonuclar = data || [];
            },
            error: (err) => {
                console.error('Müşteri hesapları çekilemedi', err);
                this.dialogHesapSonuclar = [];
            }
        });
    }

    dialogGeriDon() {
        this.dialogAsama = 1;
        this.dialogSecilenMusteri = null;
        this.dialogHesapSonuclar = [];
    }

    getMusteriAdiSoyadi(musteriId: number): string {
        const musteri = this.tumMusteriler.find(m => String(m.musteriId) === String(musteriId));
        if (musteri) {
            return (`${musteri.ad || ''} ${musteri.soyad || ''}`).trim();
        }
        return `Müşteri ID: ${musteriId}`;
    }

    hesapSec(hesap: Hesap) {
        // hesap.iban bazen camelCase bazen de PascalCase dönebilir, ikisini de kontrol et
        const iban = hesap.iban || (hesap as any).IBAN || '';
        
        if (this.secilenDialogTuru === 'sender') {
            this.secilenSenderHesap = hesap;
            this.transferForm.patchValue({ senderIban: iban });
        } else {
            this.secilenReceiverHesap = hesap;
            this.transferForm.patchValue({ receiverIban: iban });
        }
        this.dialogKapat();
    }

    hesapSifirla(tur: 'sender' | 'receiver') {
        if (tur === 'sender') {
            this.secilenSenderHesap = null;
            this.transferForm.patchValue({ senderIban: '' });
        } else {
            this.secilenReceiverHesap = null;
            this.transferForm.patchValue({ receiverIban: '' });
        }
    }

    onSubmit() {
        if (this.transferForm.valid) {
            const payload = {
                senderIban: this.transferForm.value.senderIban,
                receiverIban: this.transferForm.value.receiverIban,
                amount: this.transferForm.value.amount,
                description: this.transferForm.value.description
            };

            this.hesapService.paraTransferi(payload).subscribe({
                next: (res) => {
                    this.isTransferSuccessful = true;
                    // Dekont kartları DB'de oluşan iki hareketin ISLEM_ID'sine bağlanır.
                    this.senderTxId = res.gonderenIslemId;
                    this.receiverTxId = res.aliciIslemId;
                    this.referansNo = res.referansNo;
                    this.toastService.showSuccess(res.message, 'Transfer Başarılı');
                },
                error: (err) => {
                    this.isTransferSuccessful = false;
                    this.senderTxId = null;
                    this.receiverTxId = null;
                    this.referansNo = null;
                    this.toastService.showError(
                        err.error?.message || err.message || 'Transfer gerçekleştirilemedi.',
                        'Transfer Hatası'
                    );
                }
            });
        }
    }
}
