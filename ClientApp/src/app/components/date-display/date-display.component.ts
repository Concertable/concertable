import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-date-display',
  standalone: false,
  templateUrl: './date-display.component.html',
  styleUrl: './date-display.component.scss'
})
export class DateDisplayComponent {
  currentYear = new Date().getFullYear();

  @Input() date?: Date

  isAfterCurrentYear(date: Date): boolean {
    return date.getFullYear() > this.currentYear;
  }
}
