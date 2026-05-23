import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ApplicationCheckoutComponent } from './listing-application-checkout.component';

describe('ApplicationCheckoutComponent', () => {
  let component: ApplicationCheckoutComponent;
  let fixture: ComponentFixture<ApplicationCheckoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ApplicationCheckoutComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ApplicationCheckoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
