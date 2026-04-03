import { Component, EventEmitter, Input, Output, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-price',
  standalone: false,
  templateUrl: './price.component.html',
  styleUrl: './price.component.scss'
})
export class PriceComponent {
  @Input() price?: number;
  @Input() editMode?: boolean = false;
  @Output() priceChange = new EventEmitter<number>();

  onPriceChange(value: number | string) {
    const parsed = parseFloat(String(value));
    if (!isNaN(parsed)) {
      const rounded = parseFloat(parsed.toFixed(2)); 
      this.price = rounded;
      this.priceChange.emit(rounded);
    } else {
      this.price = undefined;
      this.priceChange.emit(undefined as any);
    }
  }
  
}
