import { Component } from '@angular/core';
import { CreateReview } from '../../models/create-review';

@Component({
  selector: 'app-add-review',
  standalone: false,
  templateUrl: './add-review.component.html',
  styleUrl: './add-review.component.scss'
})
export class AddReviewComponent {
onSubmit() {
throw new Error('Method not implemented.');
}
  review: CreateReview = {
    stars: 0,
    details: ''
  }
}
