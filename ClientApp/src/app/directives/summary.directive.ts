import { Directive, Input } from '@angular/core';
import { Event } from '../models/event';
import { Venue } from '../models/venue';
import { Artist } from '../models/artist';
import { BlobStorageService } from '../services/blob-storage/blob-storage.service';

@Directive({
  selector: '[appSummary]',
  standalone: false
})
export class SummaryDirective<T extends Artist | Event | Venue> {
  @Input() item?: T;

  constructor(protected blobService: BlobStorageService) { }
}
