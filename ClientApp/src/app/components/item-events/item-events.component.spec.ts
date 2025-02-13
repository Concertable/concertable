import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ItemEventsComponent } from './item-events.component';

describe('ItemEventsComponent', () => {
  let component: ItemEventsComponent;
  let fixture: ComponentFixture<ItemEventsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ItemEventsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ItemEventsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
