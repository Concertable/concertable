import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-date-picker',
  standalone: false,
  
  templateUrl: './date-picker.component.html',
  styleUrl: './date-picker.component.scss'
})
export class DatePickerComponent {
  @Output() dateChange = new EventEmitter<Date>();

  onDateChange(event: any) {
    const date: Date = event.value;
    this.dateChange.emit(date);
  }
}
