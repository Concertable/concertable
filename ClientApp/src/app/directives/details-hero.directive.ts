import { Directive, EventEmitter, Input, Output } from '@angular/core';
import { Artist } from '../models/artist';
import { Venue } from '../models/venue';
import { Event } from '../models/event';
import { BlobStorageService } from '../services/blob-storage/blob-storage.service';

@Directive({
  selector: '[appDetailsHero]',
  standalone: false
})
export abstract class DetailsHeroDirective<T extends Artist | Event | Venue> {
  @Input() item?: T;
  @Output() itemChange = new EventEmitter<T>();
  @Input() editMode?: boolean = false;
  @Output() latLongChange = new EventEmitter<google.maps.LatLngLiteral | undefined>();
  @Output() locationChange = new EventEmitter<{ county: string, town: string }>();

  locationEditable?: boolean= false; 
  imageEditable?: boolean = false;

  constructor(protected blobStorageService: BlobStorageService) { }

  abstract get county(): string | undefined;

  abstract set county(county: string );

  abstract get email(): string | undefined;

  abstract get town(): string | undefined;

  abstract set town(town: string);

  abstract get latitude(): number | undefined;

  abstract set latitude(latitude: number);

  abstract get longitude(): number | undefined;

  abstract set longitude(longitude: number);

  abstract get imageUrl(): string | undefined;

  abstract set imageUrl(imageUrl: string);

  get name(): string | undefined {
    return this.item?.name;
  }

  set name(name: string) {
    if (this.item)
      this.item.name = name;
  }

  get rating(): number | undefined {
    return this.item?.rating;
  }

  onChangeDetected() {
    this.itemChange.emit(this.item)
  }

  onLatLongChange(latLong?: google.maps.LatLngLiteral | undefined) {
    this.latLongChange.emit(latLong);
  }

  onLocationChange(location?: { county: string, town: string }) {
    this.locationChange.emit(location);
  }

  onImageChange(image: File) {
    return;
  }
}
