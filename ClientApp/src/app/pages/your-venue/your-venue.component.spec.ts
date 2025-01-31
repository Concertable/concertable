import { ComponentFixture, TestBed } from '@angular/core/testing';

import { YourVenueComponent } from './your-venue.component';

describe('YourVenueComponent', () => {
  let component: YourVenueComponent;
  let fixture: ComponentFixture<YourVenueComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [YourVenueComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(YourVenueComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
