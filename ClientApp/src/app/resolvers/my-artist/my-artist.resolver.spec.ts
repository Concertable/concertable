import { TestBed } from '@angular/core/testing';
import { ResolveFn } from '@angular/router';

import { myArtistResolver } from './my-artist.resolver';

describe('myArtistResolver', () => {
  const executeResolver: ResolveFn<boolean> = (...resolverParameters) => 
      TestBed.runInInjectionContext(() => myArtistResolver(...resolverParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeResolver).toBeTruthy();
  });
});
