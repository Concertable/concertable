import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-event-payment-summary',
  standalone: false,
  templateUrl: './event-payment-summary.component.html',
  styleUrl: '../../shared/components/payment-summary.component.scss'
})
export class EventPaymentSummaryComponent {
  @Input() price: number = 0;

  @Output() quantityChange = new EventEmitter<number>();
  @Output() totalChange = new EventEmitter<number>();

  quantity = 1;

  get total(): number {
    return this.price * this.quantity;
  }

  increaseQuantity() {
    this.quantity++;
    this.quantityChange.emit(this.quantity);
  }

  decreaseQuantity() {
    if (this.quantity > 1) {
      this.quantity--;
      this.quantityChange.emit(this.quantity);
    }
  }
}
