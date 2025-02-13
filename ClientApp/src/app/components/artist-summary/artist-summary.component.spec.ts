import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ArtistSummaryComponent } from './artist-summary.component';

describe('ArtistSummaryComponent', () => {
  let component: ArtistSummaryComponent;
  let fixture: ComponentFixture<ArtistSummaryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ArtistSummaryComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ArtistSummaryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
