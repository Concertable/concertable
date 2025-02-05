import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EventHeadersComponent } from './event-headers.component';

describe('EventHeadersComponent', () => {
  let component: EventHeadersComponent;
  let fixture: ComponentFixture<EventHeadersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EventHeadersComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EventHeadersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
