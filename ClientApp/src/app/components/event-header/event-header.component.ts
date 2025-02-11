import { Component, Input } from '@angular/core';
import { EventHeader } from '../../models/event-header';
import { HeaderComponent } from '../header/header.component';

@Component({
  selector: 'app-event-header',
  standalone: false,
  
  templateUrl: './event-header.component.html',
  styleUrl: './event-header.component.scss'
})
export class EventHeaderComponent extends HeaderComponent<EventHeader> {
}
