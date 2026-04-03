import { TestBed } from '@angular/core/testing';
import { ResolveFn } from '@angular/router';

import { listingApplicationResolver } from './listing-application.resolver';

describe('listingApplicationResolver', () => {
  const executeResolver: ResolveFn<boolean> = (...resolverParameters) => 
      TestBed.runInInjectionContext(() => listingApplicationResolver(...resolverParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeResolver).toBeTruthy();
  });
});
