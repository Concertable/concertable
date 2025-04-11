import { Component, Input, Output } from '@angular/core';
import { Artist } from '../../models/artist';
import { DetailsHeroDirective } from '../../directives/details-hero.directive';

@Component({
  selector: 'app-artist-details-hero',
  standalone: false,
  templateUrl: '../../shared/components/details-hero/details-hero.component.html',
  styleUrl: '../../shared/components/details-hero/details-hero.component.scss'
})
export class ArtistDetailsHeroComponent extends DetailsHeroDirective<Artist>{
  @Input() override locationEditable?: boolean = true;
  @Input() override imageEditable?: boolean = true;
  
    get artist() : Artist | undefined {
      return this.item;
    }

    @Input() set artist(artist: Artist) {
      this.item = artist;
    }

    @Output() get artistChange() {
      return this.itemChange;
    }
  
    get county(): string | undefined {
      return this.artist?.county;
    }
  
    set county(value: string) {
      if(this.artist)
        this.artist.county = value;
    }
  
    get town(): string | undefined {
      return this.artist?.town;
    }
  
    set town(value: string) {
      if(this.artist)
        this.artist.town = value;
    }
  
    get email(): string | undefined {
      return 'okok';
    }
  
    set email(value: string) {
    }
  
    get imageUrl(): string | undefined {
      console.log("ar", this.artist?.imageUrl)
      return this.artist?.imageUrl;
    }
  
    get latitude() : number | undefined {
      return undefined;
    }
  
    get longitude() : number | undefined {
      return undefined;
    }
  
    set imageUrl(value: string) {
      if(this.artist)
        this.artist.imageUrl = value;
    }
}