import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-selector',
  standalone: false,
  templateUrl: './selector.component.html',
  styleUrls: ['./selector.component.scss']
})
export class SelectorComponent<T> {
  @Input() label: string = 'Select Item';
  @Input() options: T[] = []; 
  @Input() selectedItems: T[] = []; 
  @Input() displayProperty?: keyof T;
  @Input() disabled : boolean = false
  @Input() editMode?: boolean = false;

  @Output() selectedItemsChange = new EventEmitter<T[]>(); 

  selectedValue?: T;

  addItem(): void {
    if (this.selectedValue && !this.selectedItems.includes(this.selectedValue)) {
      this.selectedItems.push(this.selectedValue); 
      this.selectedItemsChange.emit(this.selectedItems); 
    }
  }

  removeItem(item: T): void {
    const index = this.selectedItems.indexOf(item);
    if (index !== -1) {
      this.selectedItems.splice(index, 1); 
      this.selectedItemsChange.emit(this.selectedItems); 
    }
  }
}
