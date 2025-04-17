import { AfterViewChecked, AfterViewInit, Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { update } from 'lodash';

@Component({
  selector: 'app-location-search',
  standalone: false,
  
  templateUrl: './location-search.component.html',
  styleUrl: './location-search.component.scss'
})
export class LocationSearchComponent implements OnInit, AfterViewInit {
  @ViewChild('search', { static: true }) searchElement!: ElementRef;
  @Input() type: string = '(cities)';
  @Input() latitude?: number;
  @Input() longitude?: number;
  @Input() whiteOutline?: boolean;
  @Output() latLongChange = new EventEmitter<google.maps.LatLngLiteral | undefined>();
  @Output() locationChange = new EventEmitter<{county: string, town: string} | undefined>();

  locality?: string;
  county?: string;
  town?: string;
  country?: string;

  locationInput?: string;

  ngOnInit(): void {
    if (this.latitude && this.longitude) {
      const latLng = new google.maps.LatLng(this.latitude, this.longitude);
      const geocoder = new google.maps.Geocoder();

      // Update location if inputs recieved initially
      geocoder.geocode({ location: latLng }, (results, status) => {
        if (status === google.maps.GeocoderStatus.OK && results) {
          this.updateLocationInputs(results[0]?.address_components);
          if (this.country) {
            if (this.locality) 
              // If locality is available, use locality and country
              this.locationInput = `${this.locality}, ${this.country}`;
            else if (this.town) 
              // If locality is not available, use town and country
              this.locationInput = `${this.town}, ${this.country}`;
          }
        }
      })
    }
  }

  private updateLocationInputs(components: google.maps.GeocoderAddressComponent[]) {
    components.forEach(component => {
      const types = component.types;
      if (types.includes('postal_town')) 
        this.town = component.long_name;
      if (types.includes('locality'))
        this.locality = component.long_name;
      if (types.includes('country')) 
        this.country = component.short_name === 'GB' ? 'UK' : component.short_name;
      if (types.includes('administrative_area_level_2'))
        this.county = component.long_name
    });
  }
  

  ngAfterViewInit(): void {
    const options = {
      types: [this.type], // Limit results
      componentRestrictions: { country: 'gb' } // Restrict to UK
    };
  
    const autocomplete = new google.maps.places.Autocomplete(
      this.searchElement.nativeElement,
      options
    );
  
    autocomplete.addListener('place_changed', () => { // When the user selects a location from the dropdown
      const place = autocomplete.getPlace(); // Get its location
      const lat = place?.geometry?.location?.lat();
      const lng = place?.geometry?.location?.lng();
  
      if (lat && lng)
        this.latLongChange.emit({ lat, lng }); // Emit the latlong to the parent
  
      if (place?.address_components) {
        this.updateLocationInputs(place.address_components);
        if (this.county && this.town)
          this.locationChange.emit({ county: this.county, town: this.town });
      }
    });
  
    // Clear input resets values
    this.searchElement.nativeElement.addEventListener('input', () => {
      if (!this.searchElement.nativeElement.value)
        this.latLongChange.emit(undefined);
  
      this.locationInput = undefined;
    });
  }
  
}
