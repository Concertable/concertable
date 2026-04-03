import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EventHeaderCarouselComponent } from './event-header-carousel.component';

describe('EventHeaderCarouselComponent', () => {
  let component: EventHeaderCarouselComponent;
  let fixture: ComponentFixture<EventHeaderCarouselComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EventHeaderCarouselComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EventHeaderCarouselComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
