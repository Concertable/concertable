import { ChangeDetectorRef, Component, EventEmitter, Input, Output } from '@angular/core';
import { PaymentSummaryItem } from '../../models/payment-summary-item';

@Component({
  selector: 'app-payment-summary',
  standalone: false,
  templateUrl: './payment-summary.component.html',
  styleUrl: './payment-summary.component.scss'
})
export class PaymentSummaryComponent {
  @Input() title: string = 'Payment Summary';
  @Input() summaryItems: PaymentSummaryItem[] = [];
  @Output() quantityChange = new EventEmitter<number>();

  constructor(private cdr: ChangeDetectorRef) { }

  onQuantityIncrement(index: number) {
    const item = this.summaryItems[index];
    if (item && item.label === 'Quantity') {
      const quantity = Number(item.value);
      item.value = Number(item.value) + 1;
      //this.updateTotal(quantity);
      this.quantityChange.emit(item.value);
    }
  }

  onQuantityDecrement(index: number) {
    const item = this.summaryItems[index];
    if (item && item.label === 'Quantity') {
      const quantity = Number(item.value);
      if (quantity > 1) {
        item.value = quantity - 1;
        //this.updateTotal(quantity);
        this.quantityChange.emit(item.value);
      }
    }
  }

  // private updateTotal(quantity: number): void {
  //   const price = Number(this.summaryItems[0]?.value);
  //   const total = quantity * price;
  
  //   const updatedItems = [...this.summaryItems];
  //   updatedItems[2] = { ...updatedItems[2], value: total };
  
  //   this.summaryItems = updatedItems;
  // }
}
