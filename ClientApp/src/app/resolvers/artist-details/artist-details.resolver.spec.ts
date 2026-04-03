import { TestBed } from '@angular/core/testing';
import { ResolveFn } from '@angular/router';

import { artistDetailsResolver } from './artist-details.resolver';

describe('artistDetailsResolver', () => {
  const executeResolver: ResolveFn<boolean> = (...resolverParameters) => 
      TestBed.runInInjectionContext(() => artistDetailsResolver(...resolverParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeResolver).toBeTruthy();
  });
});
