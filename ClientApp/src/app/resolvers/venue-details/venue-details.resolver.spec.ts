import { TestBed } from '@angular/core/testing';
import { ResolveFn } from '@angular/router';

import { venueDetailsResolver } from '../venue-details.resolver';

describe('venueDetailsResolver', () => {
  const executeResolver: ResolveFn<boolean> = (...resolverParameters) => 
      TestBed.runInInjectionContext(() => venueDetailsResolver(...resolverParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeResolver).toBeTruthy();
  });
});
