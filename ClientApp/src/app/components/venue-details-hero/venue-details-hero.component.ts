import { Component, Input } from '@angular/core';
import { Venue } from '../../models/venue';
import { DetailsHeroDirective } from '../../directives/details-hero.directive';

@Component({
  selector: 'app-venue-details-hero',
  standalone: false,
  templateUrl: '../../shared/components/details-hero/details-hero.component.html',
  styleUrl: '../../shared/components/details-hero/details-hero.component.scss'
})
export class VenueDetailsHeroComponent extends DetailsHeroDirective<Venue>{
  @Input() venue?: Venue;
  @Input() override locationEditable?: boolean = true;
  
    get entity() {
      return this.venue;
    }
  
    get county(): string | undefined {
      return this.venue?.county;
    }
  
    set county(value: string) {
      if(this.venue)
        this.venue.county = value;
    }
  
    get town(): string | undefined {
      return this.venue?.town;
    }
  
    set town(value: string) {
      if(this.venue)
        this.venue.town = value;
    }
  
    get email(): string | undefined {
      return 'okok';
    }
  
    set email(value: string) {
    }
  
    get imageUrl(): string | undefined {
      return this.venue?.imageUrl;
    }
  
    get latitude() : number | undefined {
      return this.venue?.latitude
    }
  
    set latitude(latitude: number) {
      if(this.venue)
        this.venue.latitude = latitude;
    }
  
    get longitude() : number | undefined {
      return this.venue?.longitude
    }
  
    set longitude(longitude: number) {
      if(this.venue)
        this.venue.longitude = longitude;
    }
  
    set imageUrl(value: string) {
      if(this.venue)
        this.venue.imageUrl = value;
    }
}
