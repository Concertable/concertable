import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ConfigNavbarService {
  private heightSubject = new BehaviorSubject<number | undefined>(undefined);
  height$ = this.heightSubject.asObservable();

  setHeight(height?: number) {
    this.heightSubject.next(height);
  }
  
  getHeight(): number | undefined {
    return this.heightSubject.getValue();
  }
}
