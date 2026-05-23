import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EventDetailsHeroComponent } from './event-details-hero.component';

describe('EventDetailsHeroComponent', () => {
  let component: EventDetailsHeroComponent;
  let fixture: ComponentFixture<EventDetailsHeroComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EventDetailsHeroComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EventDetailsHeroComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
