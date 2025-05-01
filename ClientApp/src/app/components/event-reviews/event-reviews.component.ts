import { Component } from '@angular/core';
import { ReviewDirective } from '../../directives/review/review.directive';
import { Observable } from 'rxjs';
import { Pagination } from '../../models/pagination';
import { CreateReview, Review } from '../../models/review';
import { ReviewSummary } from '../../models/review-summary';
import { AddReviewComponent } from '../add-review/add-review.component';

@Component({
  selector: 'app-event-reviews',
  standalone: false,
  templateUrl: '../../shared/templates/reviews/reviews.template.html',
  styleUrl: '../../shared/templates/reviews/reviews.template.scss'
})
export class EventReviewsComponent extends ReviewDirective {
  override isEventReview: boolean = true;
  
  getSummary(id: number): Observable<ReviewSummary> {
    return this.reviewService.getSummaryByEventId(id);
  }

  get(id: number): Observable<Pagination<Review>> {
    return this.reviewService.getByEventId(id, this.pageParams);
  }

  canReview(id: number): Observable<boolean> {
      return this.reviewService.canUserReviewEvent(id);
  }

  override onAddReview(): void {
    const dialogRef = this.dialog.open(AddReviewComponent, {
      width: '500px',
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        if (this.id) {
          const review: CreateReview = {
            eventId: this.id,
            stars: result.stars,
            details: result.details
          };
    
          this.reviewService.create(review).subscribe(createdReview => {
            this.reviewsPage?.data.unshift(createdReview);
            this.toastService.showSuccess("Added Review")
          });
        }
      }
    });
    
  }
}
