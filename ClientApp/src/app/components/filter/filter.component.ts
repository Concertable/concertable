import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-filter',
  standalone: false,
  templateUrl: './filter.component.html',
  styleUrls: ['./filter.component.css']
})
export class FilterComponent {
  @Input() isOpen = false;
  @Output() close = new EventEmitter<void>();
  @Output() applyFilters = new EventEmitter<any>();

  selectedFilters = {
    option1: false,
    option2: false
  };

  closeFilter() {
    this.close.emit(); // Close filter
  }

  applyFilter() {
    this.applyFilters.emit(this.selectedFilters);
  }
}
