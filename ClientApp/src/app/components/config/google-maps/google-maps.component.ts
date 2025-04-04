import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Coordinates } from '../../../models/coordinates';

@Component({
  selector: 'app-google-maps',
  standalone: false,
  
  templateUrl: './google-maps.component.html',
  styleUrl: './google-maps.component.scss'
})
export class GoogleMapsComponent implements OnInit {
  @Input() editMode?: boolean = false;
  @Input() lat?: number;
  @Input() lng?: number;
  @Output() latLongChange = new EventEmitter<google.maps.LatLngLiteral | undefined>();
  @Output() locationChange = new EventEmitter<{ county: string, town: string} | undefined>

  options: google.maps.MapOptions = {
    center: { lat: 51, lng: 0.1 }
  }

  ngOnInit() {
    if (this.lat !== undefined && this.lng !== undefined) {
      this.options.center = { lat: this.lat, lng: this.lng };
    }
  }

  onLatLongChange(latLong: google.maps.LatLngLiteral | undefined) {
    if (latLong) {
      const { lat, lng } = latLong;
  
      this.lat = lat;
      this.lng = lng;
  
      this.latLongChange.emit({ lat, lng });
    } else {
      this.latLongChange.emit(undefined);
    }
  }
  

  onLocationChange(location?: { county: string, town: string }) {
    this.locationChange.emit(location);
  }
  
}
