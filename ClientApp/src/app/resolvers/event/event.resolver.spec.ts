import { TestBed } from '@angular/core/testing';
import { ResolveFn } from '@angular/router';

import { eventResolver } from './event.resolver';

describe('eventResolver', () => {
  const executeResolver: ResolveFn<boolean> = (...resolverParameters) => 
      TestBed.runInInjectionContext(() => eventResolver(...resolverParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeResolver).toBeTruthy();
  });
});
