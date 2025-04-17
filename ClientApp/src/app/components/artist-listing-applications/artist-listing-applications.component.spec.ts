import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ArtistListingApplicationsComponent } from './artist-listing-applications.component';

describe('ArtistListingApplicationsComponent', () => {
  let component: ArtistListingApplicationsComponent;
  let fixture: ComponentFixture<ArtistListingApplicationsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ArtistListingApplicationsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ArtistListingApplicationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
