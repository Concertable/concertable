import { TestBed } from '@angular/core/testing';
import { ResolveFn } from '@angular/router';

import { myProfileResolver } from './my-profile.resolver';

describe('myProfileResolver', () => {
  const executeResolver: ResolveFn<boolean> = (...resolverParameters) => 
      TestBed.runInInjectionContext(() => myProfileResolver(...resolverParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeResolver).toBeTruthy();
  });
});
