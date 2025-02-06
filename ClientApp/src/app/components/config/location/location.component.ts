import { Component, Input } from '@angular/core';
import { Coordinates } from '../../../models/coordinates';

@Component({
  selector: 'app-location',
  standalone: false,
  
  templateUrl: './location.component.html',
  styleUrl: './location.component.scss'
})
export class LocationComponent {
  @Input() editMode?: boolean;
  @Input() county?: string;
  @Input() town?: string;
  @Input() coordinates?: Coordinates
}
