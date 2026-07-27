import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '../../../pipes/translate.pipe';
import { HesapService } from '../../../services/hesap.service';
import { ToastService } from '../../../services/toast.service';

@Component({
    selector: 'app-hesap-transferi',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, RouterModule, TranslatePipe],
    templateUrl: './hesap-transferi.component.html',
    styleUrl: './hesap-transferi.component.css',
})
export class HesapTransferiComponent {
    transferForm: FormGroup;
    isTransferSuccessful: boolean = false;

    // Transferden dönen gerçek hareket ID'leri (dekonttaki "i" butonu bunları kullanır)
    senderTxId: number | null = null;
    receiverTxId: number | null = null;
    referansNo: string | null = null;

    constructor(
        private fb: FormBuilder,
        private hesapService: HesapService,
        private toastService: ToastService
    ) {
        this.transferForm = this.fb.group({
            senderIban: ['', [Validators.required, Validators.minLength(26), Validators.maxLength(26)]],
            receiverIban: ['', [Validators.required, Validators.minLength(26), Validators.maxLength(26)]],
            description: ['', Validators.required],
            amount: ['', [Validators.required, Validators.min(0.01)]]
        });
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
