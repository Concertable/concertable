import { TestBed } from '@angular/core/testing';
import { ResolveFn } from '@angular/router';

import { myEventsResolver } from './my-events.resolver';

describe('myEventsResolver', () => {
  const executeResolver: ResolveFn<boolean> = (...resolverParameters) => 
      TestBed.runInInjectionContext(() => myEventsResolver(...resolverParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeResolver).toBeTruthy();
  });
});
