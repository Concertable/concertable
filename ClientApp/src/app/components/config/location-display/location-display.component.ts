import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Coordinates } from '../../../models/coordinates';

@Component({
  selector: 'app-location-display',
  standalone: false,
  
  templateUrl: './location-display.component.html',
  styleUrl: './location-display.component.scss'
})
export class LocationDisplayComponent {
  @Input() editMode?: boolean;
  @Input() county?: string;
  @Input() town?: string;
  @Input() latitude?: number;
  @Input() longitude?: number;
  @Output() latLongChange = new EventEmitter<Coordinates | undefined>();

  onLatLongChange(latLong: google.maps.LatLngLiteral | undefined) {
    this.latLongChange.emit(latLong);
  }

  updateLocation(location?: { county: string, town: string }) {
    this.county = location?.county;
    this.town = location?.town;
  }
}
