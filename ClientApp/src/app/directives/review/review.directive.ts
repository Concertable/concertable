import { Directive, Input, OnInit } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { ReviewSummary } from '../../models/review-summary';
import { Pagination } from '../../models/pagination';
import { Review } from '../../models/review';
import { ReviewService } from '../../services/review/review.service';
import { PaginationParams } from '../../models/pagination-params';

@Directive({
  selector: '[appReview]',
  standalone: false
})
export abstract class ReviewDirective implements OnInit {
  @Input() id?: number; // The id depending on the details scope (e.g. artistId, eventId...)
  pageParams: PaginationParams = {};
  reviews: Review[] = [];
  summary?: ReviewSummary;
  canReview$?: Observable<boolean>

  constructor(protected reviewService: ReviewService) { }

  ngOnInit(): void {
    if(this.id)
    {
      this.getSummary(this.id).subscribe(summary => this.summary = summary);
      this.get(this.id).subscribe(reviewsPage => {
        this.pageParams.pageNumber = reviewsPage.pageNumber;
        this.pageParams.pageSize = reviewsPage.pageSize;
        this.reviews.push(...reviewsPage.data);
      });
      this.canReview$ = this.canReview(this.id);
    }
  }

  onViewMore() {
    if(this.id)
      this.get(this.id);
  }

  onAddReview() {
    throw new Error('Method not implemented.');
  }

  abstract canReview(id: number): Observable<boolean>

  abstract getSummary(id: number): Observable<ReviewSummary> 

  abstract get(id: number): Observable<Pagination<Review>> 
}
