import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ArtistDetailsHeroComponent } from './artist-details-hero.component';

describe('ArtistDetailsHeroComponent', () => {
  let component: ArtistDetailsHeroComponent;
  let fixture: ComponentFixture<ArtistDetailsHeroComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ArtistDetailsHeroComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ArtistDetailsHeroComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
