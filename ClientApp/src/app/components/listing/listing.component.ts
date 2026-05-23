import { Component, Input } from '@angular/core';
import { Listing } from '../../models/listing';

@Component({
  selector: 'app-listing',
  standalone: false,
  
  templateUrl: './listing.component.html',
  styleUrl: './listing.component.scss'
})
export class ListingComponent {
  @Input() listing?: Listing

  onDelete() {
    
  }
}
