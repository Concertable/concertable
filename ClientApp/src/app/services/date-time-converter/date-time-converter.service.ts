import { Injectable } from '@angular/core';
import { timeInterval } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DateTimeConverterService {

  toDate(time: string): Date {
    const [hours, minutes, seconds] = time.split(':').map(Number);

    const date = new Date();
    date.setHours(hours, minutes, seconds);

    return date;
  }

  toTime(date: Date): string {
    return date.toTimeString().split(' ')[0];
  }
}
