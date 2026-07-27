import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '../../../pipes/translate.pipe';
import { HesapService } from '../../../services/hesap.service';

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

    // Frontend simülasyonu için rastgele işlem ID'leri (Dekonttaki Info butonu için)
    senderTxId: number = 0;
    receiverTxId: number = 0;

    constructor(private fb: FormBuilder, private hesapService: HesapService) {
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
                    // Başarılı olduğunda, gerçekte dönen ID'leri de gösterebiliriz, şimdilik rastgele gösteriyoruz.
                    this.senderTxId = Math.floor(Math.random() * 10000) + 1000;
                    this.receiverTxId = this.senderTxId + 1;
                    alert("Transfer Başarılı: " + res.message);
                },
                error: (err) => {
                    this.isTransferSuccessful = false;
                    alert("Transfer Hatası: " + (err.error?.message || err.message));
                }
            });
        }
    }
}
