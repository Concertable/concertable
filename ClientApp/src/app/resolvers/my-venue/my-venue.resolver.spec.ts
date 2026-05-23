import { TestBed } from '@angular/core/testing';
import { ResolveFn } from '@angular/router';

import { myVenueResolver } from './my-venue.resolver';

describe('myVenueResolver', () => {
  const executeResolver: ResolveFn<boolean> = (...resolverParameters) => 
      TestBed.runInInjectionContext(() => myVenueResolver(...resolverParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeResolver).toBeTruthy();
  });
});
