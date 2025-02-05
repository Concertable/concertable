import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VenueFindComponent } from './venue-find.component';

describe('VenueFindComponent', () => {
  let component: VenueFindComponent;
  let fixture: ComponentFixture<VenueFindComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [VenueFindComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VenueFindComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
