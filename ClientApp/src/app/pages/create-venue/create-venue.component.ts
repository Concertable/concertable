import { Component } from '@angular/core';
import { Venue } from '../../models/venue';

@Component({
  selector: 'app-create-venue',
  standalone: false,
  
  templateUrl: './create-venue.component.html',
  styleUrl: './create-venue.component.scss'
})
export class CreateVenueComponent {
  venue: Venue = {
    id: 0,  
    name: "",
    about: "",
    coordinates: { latitude: 0, longitude: 0 }, 
    imageUrl: "", 
    county: "",
    town: "",
    approved: false
  }
}
