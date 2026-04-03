import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-badges',
  standalone: false,
  templateUrl: './badges.component.html',
  styleUrl: './badges.component.scss'
})
export class BadgesComponent<T> {
  @Input() items: T[] = [];
  @Input() displayProperty?: keyof T;
  @Input() editMode?: boolean = false;

  @Output() remove = new EventEmitter<T>();

  removeItem(itemToRemove: T) {
    this.remove.emit(itemToRemove);
  }
}
