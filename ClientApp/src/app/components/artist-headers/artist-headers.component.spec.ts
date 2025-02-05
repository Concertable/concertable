import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ArtistHeadersComponent } from './artist-headers.component';

describe('ArtistHeadersComponent', () => {
  let component: ArtistHeadersComponent;
  let fixture: ComponentFixture<ArtistHeadersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ArtistHeadersComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ArtistHeadersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
