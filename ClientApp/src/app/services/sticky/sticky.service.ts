import { Injectable } from '@angular/core';
import { BehaviorSubject, fromEvent, throttleTime } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class StickyService {
  private scrollYSubject = new BehaviorSubject<number>(window.scrollY);
  scrollY$ = this.scrollYSubject.asObservable();

  constructor() {
    fromEvent(window, 'scroll')
      .pipe(throttleTime(100))
      .subscribe(() => {
        this.scrollYSubject.next(window.scrollY);
      });
  }
}
