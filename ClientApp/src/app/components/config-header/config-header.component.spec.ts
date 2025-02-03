import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfigHeaderComponent } from './config-header.component';

describe('ConfigHeaderComponent', () => {
  let component: ConfigHeaderComponent;
  let fixture: ComponentFixture<ConfigHeaderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ConfigHeaderComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConfigHeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
