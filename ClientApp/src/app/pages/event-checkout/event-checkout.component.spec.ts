import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TicketCheckoutComponent } from './event-checkout.component';

describe('TicketCheckoutComponent', () => {
  let component: TicketCheckoutComponent;
  let fixture: ComponentFixture<TicketCheckoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TicketCheckoutComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TicketCheckoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
