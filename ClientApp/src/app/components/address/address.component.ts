import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-address',
  standalone: false,
  templateUrl: './address.component.html',
  styleUrl: './address.component.scss'
})
export class AddressComponent implements OnChanges {
  @Input() lat?: number;
  @Input() lng?: number;

  fullAddress: string = 'Loading address...';

  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['lat'] || changes['lng']) && this.lat !== undefined && this.lng !== undefined) {
      this.lookupAddress();
    }
  }

  lookupAddress() {
    if (typeof google !== 'undefined') {
      const geocoder = new google.maps.Geocoder();
      const latLng = new google.maps.LatLng(this.lat!, this.lng!);
  
      geocoder.geocode({ location: latLng }, (results, status) => {
        if (status === google.maps.GeocoderStatus.OK && results && results.length > 0) {
          let selectedResult;
  
          console.log(results)
          // Try establishment first
          for (const result of results) {
            if (result.types.includes('establishment')) {
              selectedResult = result;
              break;
            }
          }
  
          // If no establishment found, try route
          if (!selectedResult) {
            for (const result of results) {
              if (result.types.includes('route')) {
                selectedResult = result;
                break;
              }
            }
          }
  
          // Fallback to first result
          if (!selectedResult) {
            selectedResult = results[0];
          }
  
          this.fullAddress = selectedResult.formatted_address;
        } else {
          this.fullAddress = 'Address not found';
        }
      });
    } else {
      this.fullAddress = 'Invalid coordinates';
    }
  }  
}