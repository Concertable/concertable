import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VenueDetailsHeroComponent } from './venue-details-hero.component';

describe('VenueDetailsHeroComponent', () => {
  let component: VenueDetailsHeroComponent;
  let fixture: ComponentFixture<VenueDetailsHeroComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [VenueDetailsHeroComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VenueDetailsHeroComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
