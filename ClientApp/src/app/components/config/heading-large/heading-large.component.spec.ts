import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HeadingLargeComponent } from './heading-large.component';

describe('HeadingLargeComponent', () => {
  let component: HeadingLargeComponent;
  let fixture: ComponentFixture<HeadingLargeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [HeadingLargeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HeadingLargeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
