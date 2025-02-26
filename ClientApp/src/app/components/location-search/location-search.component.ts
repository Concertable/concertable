import { AfterViewChecked, AfterViewInit, Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';

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
  @Output() locationChange = new EventEmitter<google.maps.LatLngLiteral | undefined>();

  locationInput?: string;

  ngOnInit(): void {
    if (this.latitude && this.longitude) {
      const latLng = new google.maps.LatLng(this.latitude, this.longitude);
    const geocoder = new google.maps.Geocoder();

    geocoder.geocode({ location: latLng }, (results, status) => {
      if (status === google.maps.GeocoderStatus.OK && results) {
        let locality: string | undefined;
        let country: string | undefined;
        results[0]?.address_components.forEach((component: any) => {
          console.log(component);
          if (component.types.includes('postal_town')) {
            locality = component.long_name; 
          }
          if (component.types.includes('country')) {
            country = component.short_name === 'GB' ? 'UK' : component.short_name;
          }
          if(locality && country)
            this.locationInput = `${locality}, ${country}`
        });
      }
    });
    }
  }

  ngAfterViewInit(): void {
    const options = {
      types: [this.type], // This limits results to cities. Adjust if needed.
      componentRestrictions: { country: 'gb' } // 'gb' restricts to the United Kingdom.
    };

    const autocomplete = new google.maps.places.Autocomplete(this.searchElement.nativeElement, options);

    // Listen for the place_changed event
    autocomplete.addListener('place_changed', () => {
      const place = autocomplete.getPlace();
      if (place && place.geometry) {
        const lat = place.geometry.location?.lat();
        const lng = place.geometry.location?.lng();
        if(lat && lng)
        {
          this.locationChange.emit({lat, lng});
          this.locationInput = place.formatted_address;
        }
      }
    });

    this.searchElement.nativeElement.addEventListener('input', () => {
      if (!this.searchElement.nativeElement.value) {
        this.locationChange.emit(undefined); // Emit null when input is cleared
      }
      this.locationInput = undefined;
    });
  }
}
