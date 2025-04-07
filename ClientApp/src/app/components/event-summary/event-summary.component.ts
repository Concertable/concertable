import { Component, Input } from '@angular/core';
import { Event } from '../../models/event';
import { SummaryDirective } from '../../directives/summary.directive';

@Component({
  selector: 'app-event-summary',
  standalone: false,
  templateUrl: './event-summary.component.html',
  styleUrl: './event-summary.component.scss'
})
export class EventSummaryComponent extends SummaryDirective<Event> {
  @Input() event?: Event
}
