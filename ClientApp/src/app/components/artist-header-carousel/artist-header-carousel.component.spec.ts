import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ArtistHeaderCarouselComponent } from './artist-header-carousel.component';

describe('ArtistHeaderCarouselComponent', () => {
  let component: ArtistHeaderCarouselComponent;
  let fixture: ComponentFixture<ArtistHeaderCarouselComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ArtistHeaderCarouselComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ArtistHeaderCarouselComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
