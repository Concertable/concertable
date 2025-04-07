import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, ViewChild } from '@angular/core';
import { Coordinates } from '../../../models/coordinates';
import { GoogleMap } from '@angular/google-maps';

@Component({
  selector: 'app-google-maps',
  standalone: false,
  templateUrl: './google-maps.component.html',
  styleUrl: './google-maps.component.scss'
})
export class GoogleMapsComponent implements OnInit, OnChanges {
logEvent() {
console.log("hey")
}
  @Input() editMode?: boolean = false;
  @Input() lat?: number;
  @Input() lng?: number;
  @Output() latLongChange = new EventEmitter<google.maps.LatLngLiteral | undefined>();
  @Output() locationChange = new EventEmitter<{ county: string, town: string} | undefined>

  @ViewChild(GoogleMap) map?: GoogleMap;

  options: google.maps.MapOptions = {
    center: { lat: 51, lng: 0.1 }
  }

  ngOnInit() {
    if (this.lat !== undefined && this.lng !== undefined) {
      this.options.center = { lat: this.lat, lng: this.lng };
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['lat'] || changes['lng']) {
      if (this.lat !== undefined && this.lng !== undefined) {
        this.options.center = { lat: this.lat, lng: this.lng };
        if (this.map) {
          this.map.panTo(this.options.center);
        }
      }
    }
  }

  onLatLongChange(latLong: google.maps.LatLngLiteral | undefined) {
    console.log("312")
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
