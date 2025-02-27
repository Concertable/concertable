import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ArtistReviewsComponent } from './artist-reviews.component';

describe('ArtistReviewsComponent', () => {
  let component: ArtistReviewsComponent;
  let fixture: ComponentFixture<ArtistReviewsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ArtistReviewsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ArtistReviewsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
