import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-time-picker',
  standalone: false,
  templateUrl: './time-picker.component.html',
  styleUrl: './time-picker.component.scss'
})
export class TimePickerComponent {
  @Input() time?: Date;
  @Output() timeChange = new EventEmitter<Date>();

  onTimeChange() {
    if(this.time)
      this.timeChange.emit(this.time);
  }
}
