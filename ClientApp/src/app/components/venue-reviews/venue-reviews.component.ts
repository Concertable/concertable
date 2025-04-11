import { Component } from '@angular/core';
import { ReviewDirective } from '../../directives/review/review.directive';
import { Observable } from 'rxjs';
import { Pagination } from '../../models/pagination';
import { Review } from '../../models/review';
import { ReviewSummary } from '../../models/review-summary';

@Component({
  selector: 'app-venue-reviews',
  standalone: false,
  templateUrl: '../../shared/templates/reviews/reviews.template.html',
  styleUrl: '../../shared/templates/reviews/reviews.template.scss'
})
export class VenueReviewsComponent extends ReviewDirective{
  getSummary(id: number): Observable<ReviewSummary> {
    return this.reviewService.getSummaryByVenueId(id);
  }

  get(id: number): Observable<Pagination<Review>> {
    return this.reviewService.getByVenueId(id, this.pageParams)
  }

  canReview(id: number): Observable<boolean> {
    return this.reviewService.canUserReviewVenue(id);
  }

  override onAddReview() {
    super.onAddReview()
      this.dialogRef?.afterClosed().subscribe(result => {
        if (result) {
          console.log('Review submitted:', result);
          // TODO: Emit to parent, send to API, or update state
        }
      });
  }
}
