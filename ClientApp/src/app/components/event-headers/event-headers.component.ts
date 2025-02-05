import { Component, Input } from '@angular/core';
import { EventHeader } from '../../models/event-header';

@Component({
  selector: 'app-event-headers',
  standalone: false,
  
  templateUrl: './event-headers.component.html',
  styleUrl: './event-headers.component.scss'
})
export class EventHeadersComponent {
  @Input() headers: EventHeader[] = [];
}
