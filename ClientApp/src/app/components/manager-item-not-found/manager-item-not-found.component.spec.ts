import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManagerItemNotFoundComponent } from './manager-item-not-found.component';

describe('ManagerItemNotFoundComponent', () => {
  let component: ManagerItemNotFoundComponent;
  let fixture: ComponentFixture<ManagerItemNotFoundComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ManagerItemNotFoundComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManagerItemNotFoundComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
