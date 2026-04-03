import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VenueReviewsComponent } from './venue-reviews.component';

describe('VenueReviewsComponent', () => {
  let component: VenueReviewsComponent;
  let fixture: ComponentFixture<VenueReviewsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [VenueReviewsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VenueReviewsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
