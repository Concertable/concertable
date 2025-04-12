import { Component } from '@angular/core';
import { CreateReview } from '../../models/create-review';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-add-review',
  standalone: false,
  templateUrl: './add-review.component.html',
  styleUrl: './add-review.component.scss'
})
export class AddReviewComponent {
  review: CreateReview = {
    stars: 0,
    details: ''
  }

  constructor(private dialogRef: MatDialogRef<AddReviewComponent>) { }

  onSubmit() {
    if (this.review.stars && this.review.details?.trim()) {
      this.dialogRef.close(this.review); // Send data back to subscriber
    }
  }

  onCancel() {
    this.dialogRef.close()
  }
}
