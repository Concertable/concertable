import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HeadingMediumComponent } from './heading-medium.component';

describe('HeadingMediumComponent', () => {
  let component: HeadingMediumComponent;
  let fixture: ComponentFixture<HeadingMediumComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [HeadingMediumComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HeadingMediumComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
