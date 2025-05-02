import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { EventHeader } from '../../models/event-header';
import { EventService } from '../event/event.service';
import { SignalRService } from '../signalr/signalr.service';
import { EventToastService } from '../toast/event/event-toast.service';

@Injectable({
  providedIn: 'root'
})
export class RecommendedEventsService {
  private headersSubject = new ReplaySubject<EventHeader>(10);
  headers$ = this.headersSubject.asObservable();

  private initialized = false;

  constructor(
    private eventService: EventService,
    private signalRService: SignalRService,
    private toastService: EventToastService
  ) {}

  init(): void {
    if (this.initialized) return;
    this.initialized = true;

    this.eventService.getRecommendedHeaders().subscribe(headers => {
      headers.forEach(header => this.headersSubject.next(header));
    });

    this.signalRService.eventPosted$.subscribe(header => {
      if (header) {
        this.toastService.showRecommended(header.id);
        this.headersSubject.next(header);
      }
    });
  }

  addFakeEvent(header: EventHeader): void {
    this.headersSubject.next(header);
  }
}
