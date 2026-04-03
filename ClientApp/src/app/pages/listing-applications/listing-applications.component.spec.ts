import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListingApplicationsComponent } from './listing-applications.component';

describe('ListingApplicationsComponent', () => {
  let component: ListingApplicationsComponent;
  let fixture: ComponentFixture<ListingApplicationsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ListingApplicationsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListingApplicationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
