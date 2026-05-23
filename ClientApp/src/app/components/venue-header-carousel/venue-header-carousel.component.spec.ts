import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VenueHeaderCarouselComponent } from './venue-header-carousel.component';

describe('VenueHeaderCarouselComponent', () => {
  let component: VenueHeaderCarouselComponent;
  let fixture: ComponentFixture<VenueHeaderCarouselComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [VenueHeaderCarouselComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VenueHeaderCarouselComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
