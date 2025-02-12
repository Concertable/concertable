import { AfterViewChecked, AfterViewInit, Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';

@Component({
  selector: 'app-location-search',
  standalone: false,
  
  templateUrl: './location-search.component.html',
  styleUrl: './location-search.component.scss'
})
export class LocationSearchComponent implements AfterViewInit {
  @ViewChild('search', { static: true }) searchElement!: ElementRef;
  @Input() type: string = '(cities)';
  @Output() locationChange = new EventEmitter<{ lat: number; lng: number }>();

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
          this.locationChange.emit({lat, lng});
      }
    });
  }
}
