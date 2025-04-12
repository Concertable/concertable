import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Artist } from '../../models/artist';
import { ActivatedRoute, Router } from '@angular/router';
import { SummaryDirective } from '../../directives/summary.directive';
import { BlobStorageService } from '../../services/blob-storage/blob-storage.service';

@Component({
  selector: 'app-artist-summary',
  standalone: false,
  templateUrl: './artist-summary.component.html',
  styleUrl: './artist-summary.component.scss'
})
export class ArtistSummaryComponent extends SummaryDirective<Artist> {
  @Input() artist?: Artist;
  @Input() showAccept?: boolean;
  @Output() accept = new EventEmitter<void>

  constructor(private router: Router, private route: ActivatedRoute, blobService: BlobStorageService) {
    super(blobService);
   }

  onViewDetails() {
    if(this.artist)
      this.router.navigate([`/find/artist`, this.artist.id]);
  }

  onAccept() {
    this.accept.emit();
  }
  
}
