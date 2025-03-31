import { Component } from '@angular/core';
import { ReviewDirective } from '../../directives/review/review.directive';
import { Observable } from 'rxjs';
import { Pagination } from '../../models/pagination';
import { Review } from '../../models/review';
import { ReviewSummary } from '../../models/review-summary';

@Component({
  selector: 'app-artist-reviews',
  standalone: false,
  templateUrl: '../../shared/templates/reviews/reviews.template.html',
  styleUrl: '../../shared/templates/reviews/reviews.template.scss'
})
export class ArtistReviewsComponent extends ReviewDirective {
  getSummary(id: number): Observable<ReviewSummary> {
    return this.reviewService.getSummaryByArtistId(id);
  }
  get(id: number): Observable<Pagination<Review>> {
    return this.reviewService.getByArtistId(id, this.pageParams);
  }

  canReview(id: number): Observable<boolean> {
      return this.reviewService.canUserReviewArtist(id);
  }
}
