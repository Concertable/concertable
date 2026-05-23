import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EventPaymentSummaryComponent } from './event-payment-summary.component';

describe('EventPaymentSummaryComponent', () => {
  let component: EventPaymentSummaryComponent;
  let fixture: ComponentFixture<EventPaymentSummaryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EventPaymentSummaryComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EventPaymentSummaryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
