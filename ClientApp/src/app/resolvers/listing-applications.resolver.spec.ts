import { TestBed } from '@angular/core/testing';
import { ResolveFn } from '@angular/router';

import { listingApplicationsResolver } from './listing-applications.resolver';

describe('listingApplicationsResolver', () => {
  const executeResolver: ResolveFn<boolean> = (...resolverParameters) => 
      TestBed.runInInjectionContext(() => listingApplicationsResolver(...resolverParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeResolver).toBeTruthy();
  });
});
