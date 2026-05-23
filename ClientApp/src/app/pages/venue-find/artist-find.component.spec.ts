import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ArtistFindComponent } from './artist-find.component';

describe('VenueFindComponent', () => {
  let component: ArtistFindComponent;
  let fixture: ComponentFixture<ArtistFindComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ArtistFindComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ArtistFindComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
