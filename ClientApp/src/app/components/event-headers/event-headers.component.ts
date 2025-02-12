import { Component, Input } from '@angular/core';
import { EventHeader } from '../../models/event-header';
import { HeadersComponent } from '../headers/headers.component';
import { HeaderType } from '../../models/header-type';

@Component({
  selector: 'app-event-headers',
  standalone: false,
  
  templateUrl: './event-headers.component.html',
  styleUrl: './event-headers.component.scss'
})
export class EventHeadersComponent extends HeadersComponent<EventHeader> {
  headerType: HeaderType = 'event';
}
