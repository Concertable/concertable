import { TestBed } from '@angular/core/testing';
import { ResolveFn } from '@angular/router';

import { eventDetailsResolver } from './event-details.resolver';

describe('eventDetailsResolver', () => {
  const executeResolver: ResolveFn<boolean> = (...resolverParameters) => 
      TestBed.runInInjectionContext(() => eventDetailsResolver(...resolverParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeResolver).toBeTruthy();
  });
});
