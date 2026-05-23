import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-badge',
  standalone: false,
  templateUrl: './badge.component.html',
  styleUrl: './badge.component.scss'
})
export class BadgeComponent<T> {
  @Input() displayProperty?: keyof T;
  @Input() editMode?: boolean = false;  
  @Input() item: T | undefined;  

  @Output() remove = new EventEmitter<T>();  

  removeBadge() {
    if (this.item) {
      this.remove.emit(this.item);  
    }
  }
}
