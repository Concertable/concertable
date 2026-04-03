import { Component } from '@angular/core';
import { ReviewDirective } from '../../directives/review/review.directive';
import { Observable } from 'rxjs';
import { Pagination } from '../../models/pagination';
import { Review } from '../../models/review';
import { ReviewSummary } from '../../models/review-summary';
import { ToastService } from '../../services/toast/toast.service';
import { ReviewService } from '../../services/review/review.service';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-artist-reviews',
  standalone: false,
  templateUrl: '../../shared/templates/reviews/reviews.template.html',
  styleUrl: '../../shared/templates/reviews/reviews.template.scss'
})
export class ArtistReviewsComponent extends ReviewDirective {
  constructor(reviewService: ReviewService,
    dialog: MatDialog,
    toastService: ToastService
  ) {
    super(reviewService, dialog, toastService)
  }

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
