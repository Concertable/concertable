import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VenueHeadersComponent } from './venue-headers.component';

describe('VenueHeadersComponent', () => {
  let component: VenueHeadersComponent;
  let fixture: ComponentFixture<VenueHeadersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [VenueHeadersComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VenueHeadersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
