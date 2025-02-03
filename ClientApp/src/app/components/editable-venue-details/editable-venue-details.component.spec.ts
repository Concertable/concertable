import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditableVenueDetailsComponent } from './editable-venue-details.component';

describe('EditableVenueDetailsComponent', () => {
  let component: EditableVenueDetailsComponent;
  let fixture: ComponentFixture<EditableVenueDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EditableVenueDetailsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditableVenueDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
