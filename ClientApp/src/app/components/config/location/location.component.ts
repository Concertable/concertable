import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-location',
  standalone: false,
  templateUrl: './location.component.html',
  styleUrl: './location.component.scss'
})
export class LocationComponent {
  @Input() county?: string;
  @Input() town?: string;
  @Input() latitude?: number;
  @Input() longitude?: number;
  @Input() whiteOutline?: boolean;
  @Input() editMode?: boolean = false;

  @Output() latLongChange = new EventEmitter<google.maps.LatLngLiteral|undefined>();
  @Output() locationChange = new EventEmitter<{ county: string, town: string }>();

  onLatLongChange(latLong: google.maps.LatLngLiteral|undefined) {
    this.latLongChange.emit(latLong);
  }

  onLocationChange(location?: { county: string, town: string }) {
    this.locationChange.emit(location);
  }
}
