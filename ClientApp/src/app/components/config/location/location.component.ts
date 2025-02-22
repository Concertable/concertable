import { Component, EventEmitter, Input, Output } from '@angular/core';
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
  @Input() latitude?: number;
  @Input() longitude?: number;
  @Output() coordinatesChange = new EventEmitter<Coordinates | undefined>();

  onCoordinatesChange(coordinates: Coordinates | undefined) {
    this.coordinatesChange.emit(coordinates);
  }
}
