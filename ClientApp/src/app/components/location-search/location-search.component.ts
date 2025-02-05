import { AfterViewChecked, AfterViewInit, Component, ElementRef, ViewChild } from '@angular/core';

@Component({
  selector: 'app-location-search',
  standalone: false,
  
  templateUrl: './location-search.component.html',
  styleUrl: './location-search.component.scss'
})
export class LocationSearchComponent implements AfterViewInit {
  @ViewChild('search', { static: true }) searchElement!: ElementRef;

  ngAfterViewInit(): void {
    const options = {
      types: ['(cities)'], // This limits results to cities. Adjust if needed.
      componentRestrictions: { country: 'gb' } // 'gb' restricts to the United Kingdom.
    };

    const autocomplete = new google.maps.places.Autocomplete(this.searchElement.nativeElement, options);

    // Listen for the place_changed event
    autocomplete.addListener('place_changed', () => {
      const place = autocomplete.getPlace();
      if (place && place.geometry) {
        console.log('Place name:', place.name);
        console.log('Latitude:', place.geometry.location?.lat());
        console.log('Longitude:', place.geometry.location?.lng());
      }
    });
  }
}
