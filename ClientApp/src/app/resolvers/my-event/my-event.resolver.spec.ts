import { TestBed } from '@angular/core/testing';
import { ResolveFn } from '@angular/router';

import { myEventResolver } from './my-event.resolver';

describe('myEventResolver', () => {
  const executeResolver: ResolveFn<boolean> = (...resolverParameters) => 
      TestBed.runInInjectionContext(() => myEventResolver(...resolverParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeResolver).toBeTruthy();
  });
});
