import { Directive, Input, OnInit } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Observable } from 'rxjs';
import { ReviewSummary } from '../../models/review-summary';
import { Pagination } from '../../models/pagination';
import { Review } from '../../models/review';
import { ReviewService } from '../../services/review/review.service';
import { PaginationParams } from '../../models/pagination-params';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { AddReviewComponent } from '../../components/add-review/add-review.component';
import { ToastService } from '../../services/toast/toast.service';
import { EventReviewsComponent } from '../../components/event-reviews/event-reviews.component';
import { PaginationHandler } from '../../shared/handler/pagination-handler';

@Directive({
  selector: '[appReview]',
  standalone: false
})
export abstract class ReviewDirective extends PaginationHandler<Review> implements OnInit {
  @Input() id?: number;
  protected dialogRef?: MatDialogRef<AddReviewComponent, Review | undefined>;

  totalReviews: number = 0;
  reviewsPage?: Pagination<Review>;
  summary?: ReviewSummary;
  canReview$?: Observable<boolean>;
  isEventReview = false;

  constructor(protected reviewService: ReviewService, protected dialog: MatDialog, protected toastService: ToastService) {
    super()
  }

  ngOnInit(): void {
    if (this.id) {
      this.getSummary(this.id).subscribe(summary => (this.summary = summary));
      this.loadPage();
      this.canReview$ = this.canReview(this.id);
    }
  }

  loadPage(): void {
    if (!this.id) return;
  
    this.get(this.id).subscribe(reviewsPage => {
      this.reviewsPage = reviewsPage;
    });
  }

  abstract canReview(id: number): Observable<boolean>;

  abstract getSummary(id: number): Observable<ReviewSummary>;

  abstract get(id: number): Observable<Pagination<Review>>;

  onAddReview() {

  }
}
