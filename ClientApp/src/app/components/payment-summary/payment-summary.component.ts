import { Component, EventEmitter, Input, Output } from '@angular/core';
import { PaymentSummaryItem } from '../../models/payment-summary-item';

@Component({
  selector: 'app-payment-summary',
  standalone: false,
  templateUrl: './payment-summary.component.html',
  styleUrl: './payment-summary.component.scss'
})
export class PaymentSummaryComponent {
  @Input() title: string = 'Payment Summary';
  @Input() summaryItems: PaymentSummaryItem[] = []
  @Output() quantityChange = new EventEmitter<number>();

  onQuantityIncrement(index: number) {
    const item = this.summaryItems[index];
    if (item && item.label === 'Quantity') {
      item.value = Number(item.value) + 1;
      this.quantityChange.emit(item.value);
    }
  }

  onQuantityDecrement(index: number) {
    const item = this.summaryItems[index];
    if (item && item.label === 'Quantity') {
      const quantity = Number(item.value);
      if (quantity > 1) {
        item.value = quantity - 1;
        this.quantityChange.emit(item.value);
      }
    }
  }
  
}
