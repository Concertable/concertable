import { AfterViewChecked, AfterViewInit, Component, ElementRef, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, ViewChild } from '@angular/core';
import { update } from 'lodash';

@Component({
  selector: 'app-location-search',
  standalone: false,
  
  templateUrl: './location-search.component.html',
  styleUrl: './location-search.component.scss'
})
export class LocationSearchComponent implements AfterViewInit, OnChanges {
  @ViewChild('search', { static: true }) searchElement!: ElementRef;
  @Input() type: string = '(cities)';
  @Input() latitude?: number;
  @Input() longitude?: number;
  @Input() whiteOutline?: boolean;

  @Output() latLongChange = new EventEmitter<google.maps.LatLngLiteral | undefined>();
  @Output() locationChange = new EventEmitter<{ county: string, town: string } | undefined>();

  locality?: string;
  county?: string;
  town?: string;
  country?: string;

  locationInput?: string;

  ngOnChanges(changes: SimpleChanges): void {
    if (
      (changes['latitude']) ||
      (changes['longitude'])
    ) {
      if (this.latitude && this.longitude) {
        this.resolveLocationFromCoords(this.latitude, this.longitude);
      }
    }
  }

  private resolveLocationFromCoords(lat: number, lng: number): void {
    const latLng = new google.maps.LatLng(lat, lng);
    const geocoder = new google.maps.Geocoder();

    geocoder.geocode({ location: latLng }, (results, status) => {
      if (status === google.maps.GeocoderStatus.OK && results) {
        this.updateLocationInputs(results[0]?.address_components);
        if (this.country) {
          if (this.locality) {
            this.locationInput = `${this.locality}, ${this.country}`;
          } else if (this.town) {
            this.locationInput = `${this.town}, ${this.country}`;
          }
        }
      }
    });
  }

  private updateLocationInputs(components: google.maps.GeocoderAddressComponent[]) {
    components.forEach(component => {
      const types = component.types;
      if (types.includes('postal_town')) this.town = component.long_name;
      if (types.includes('locality')) this.locality = component.long_name;
      if (types.includes('country')) {
        this.country = component.short_name === 'GB' ? 'UK' : component.short_name;
      }
      if (types.includes('administrative_area_level_2')) this.county = component.long_name;
    });
  }

  ngAfterViewInit(): void {
    const options = {
      types: [this.type],
      componentRestrictions: { country: 'gb' }
    };

    const autocomplete = new google.maps.places.Autocomplete(
      this.searchElement.nativeElement,
      options
    );

    autocomplete.addListener('place_changed', () => {
      const place = autocomplete.getPlace();
      const lat = place?.geometry?.location?.lat();
      const lng = place?.geometry?.location?.lng();

      if (lat && lng) {
        this.latLongChange.emit({ lat, lng });
      }

      if (place?.address_components) {
        this.updateLocationInputs(place.address_components);
        if (this.county && this.town) {
          this.locationChange.emit({ county: this.county, town: this.town });
        }
      }
    });

    // Clear input resets values
    this.searchElement.nativeElement.addEventListener('input', () => {
      if (!this.searchElement.nativeElement.value) {
        this.latLongChange.emit(undefined);
        this.locationInput = undefined;
      }
    });
  }
}
