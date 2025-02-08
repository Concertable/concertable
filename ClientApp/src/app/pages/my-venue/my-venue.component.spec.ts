import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyVenueComponent } from './my-venue.component';

describe('YourVenueComponent', () => {
  let component: MyVenueComponent;
  let fixture: ComponentFixture<MyVenueComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MyVenueComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyVenueComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
