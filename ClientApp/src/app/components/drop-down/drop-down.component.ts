import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-drop-down',
  standalone: false,
  templateUrl: './drop-down.component.html',
  styleUrl: './drop-down.component.scss'
})
export class DropDownComponent<T> {
  @Input() label?: string;
  @Input() options: T[] = [];
  @Input() selectedValue?: T; 
  @Input() displayProperty?: keyof T;
  @Input() disabled: boolean = false;
  @Output() selectionChange = new EventEmitter<T>();

  onSelectionChange(value: T) {
    this.selectionChange.emit(value);
  }
}
