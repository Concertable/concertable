import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-listing-application-payment-summary',
  standalone: false,
  templateUrl: './listing-application-payment-summary.component.html',
  styleUrl: '../../shared/components/payment-summary.component.scss'
})
export class ListingApplicationPaymentSummaryComponent {
  @Input() artistName?: string = '';
  @Input() amount?: number = 0;
}
