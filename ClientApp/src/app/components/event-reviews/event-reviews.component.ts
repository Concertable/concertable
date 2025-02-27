import { Component } from '@angular/core';
import { ReviewDirective } from '../../directives/review.directive';
import { Observable } from 'rxjs';
import { Pagination } from '../../models/pagination';
import { Review } from '../../models/review';
import { ReviewSummary } from '../../models/review-summary';

@Component({
  selector: 'app-event-reviews',
  standalone: false,
  templateUrl: '../../shared/templates/reviews/reviews.template.html',
  styleUrl: '../../shared/templates/reviews/reviews.template.scss'
})
export class EventReviewsComponent extends ReviewDirective {
  getSummary(id: number): Observable<ReviewSummary> {
    return this.reviewService.getSummaryByEventId(id);
  }
  get(id: number): Observable<Pagination<Review>> {
    return this.reviewService.getByEventId(id, this.pageParams);
  }
}
