import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListingApplicationPaymentSummaryComponent } from './listing-application-payment-summary.component';

describe('ListingApplicationPaymentSummaryComponent', () => {
  let component: ListingApplicationPaymentSummaryComponent;
  let fixture: ComponentFixture<ListingApplicationPaymentSummaryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ListingApplicationPaymentSummaryComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListingApplicationPaymentSummaryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
