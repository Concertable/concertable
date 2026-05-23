import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreatePreferencesComponent } from './create-preference.component';

describe('CreatePreferencesComponent', () => {
  let component: CreatePreferencesComponent;
  let fixture: ComponentFixture<CreatePreferencesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CreatePreferencesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreatePreferencesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
