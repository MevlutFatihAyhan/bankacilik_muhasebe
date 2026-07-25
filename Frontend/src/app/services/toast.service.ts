import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Toast, ToastType } from '../models/toast.model';

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toastsSubject = new BehaviorSubject<Toast[]>([]);
  public toasts$: Observable<Toast[]> = this.toastsSubject.asObservable();
  private nextId = 1;

  public show(message: string, type: ToastType = 'info', title?: string, duration: number = 4000): void {
    const id = this.nextId++;
    const toast: Toast = { id, type, title, message, duration };
    
    const currentToasts = this.toastsSubject.getValue();
    this.toastsSubject.next([...currentToasts, toast]);

    if (duration > 0) {
      setTimeout(() => {
        this.remove(id);
      }, duration);
    }
  }

  public showSuccess(message: string, title: string = 'Başarılı'): void {
    this.show(message, 'success', title);
  }

  public showError(message: string, title: string = 'Hata'): void {
    this.show(message, 'error', title, 5000);
  }

  public showInfo(message: string, title: string = 'Bilgi'): void {
    this.show(message, 'info', title);
  }

  public showWarning(message: string, title: string = 'Uyarı'): void {
    this.show(message, 'warning', title);
  }

  public remove(id: number): void {
    const currentToasts = this.toastsSubject.getValue();
    this.toastsSubject.next(currentToasts.filter(t => t.id !== id));
  }
}
