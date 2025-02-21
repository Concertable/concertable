import { Component, Input, OnInit } from '@angular/core';
import { ReviewService } from '../../services/review/review.service';
import { ReviewSummary } from '../../models/review-summary';
import { Observable } from 'rxjs';
import { Item } from '../../models/item';
import { ItemType } from '../../models/item-type';
import { Review } from '../../models/review';
import { Artist } from '../../models/artist';

@Component({
  selector: 'app-review-summary',
  standalone: false,
  templateUrl: './review-summary.component.html',
  styleUrl: './review-summary.component.scss'
})
export class ReviewSummaryComponent implements OnInit {
  @Input() item?: Item;
  
  constructor(private reviewService: ReviewService) { }

  private summaryMethods: Record<ItemType, (id: number) => Observable<ReviewSummary>> = {
    artist: (id) => this.reviewService.getSummaryByArtistId(id),
    event: (id) => this.reviewService.getSummaryByEventId(id),
    venue: (id) => this.reviewService.getSummaryByVenueId(id),
  };

  ngOnInit(): void {
      this.getSummary();
  }

  getSummary() {
    if(this.item && this.item)
    {
      this.summaryMethods[this.item.type](this.item.id).subscribe(r => console.log(r))
    }
  }
}
