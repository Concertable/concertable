import { Injectable } from '@angular/core';

export interface DateFields {
  startDate: Date | string;
  endDate: Date | string;
}

@Injectable({
  providedIn: 'root'
})
export class DateService {
  currentYear = new Date().getFullYear();

  constructor() { }

  private convertToDate(date: Date | string): Date {
    return new Date(date); 
  }

  // Converts both startDate and endDate of a single object to Date
  convertItemToDates<T extends DateFields>(item: T): void {
    item.startDate = this.convertToDate(item.startDate);
    item.endDate = this.convertToDate(item.endDate);
  }

  // Converts startDate and endDate for all items in an array
  convertItemsToDates<T extends DateFields>(items: T[]): void {
    items.forEach(item => {
      item.startDate = this.convertToDate(item.startDate);
      item.endDate = this.convertToDate(item.endDate);
    });
  }

  // Check if the date is after the current year
  isAfterCurrentYear(date: Date): boolean {
    return date.getFullYear() > this.currentYear;
  }

  formatDate(date: Date, format: string): string {
    const options: Intl.DateTimeFormatOptions = { year: 'numeric', month: 'short', day: 'numeric' };
    return new Intl.DateTimeFormat('en-GB', options).format(date);
  }
}
