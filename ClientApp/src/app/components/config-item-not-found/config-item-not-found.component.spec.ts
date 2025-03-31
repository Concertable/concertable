import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfigItemNotFoundComponent } from './config-item-not-found.component';

describe('ManagerItemNotFoundComponent', () => {
  let component: ConfigItemNotFoundComponent;
  let fixture: ComponentFixture<ConfigItemNotFoundComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ConfigItemNotFoundComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConfigItemNotFoundComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
