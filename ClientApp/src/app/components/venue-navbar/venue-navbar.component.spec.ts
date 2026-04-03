import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VenueHeaderComponent } from './venue-navbar.component';

describe('VenueHeaderComponent', () => {
  let component: VenueHeaderComponent;
  let fixture: ComponentFixture<VenueHeaderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [VenueHeaderComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VenueHeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
