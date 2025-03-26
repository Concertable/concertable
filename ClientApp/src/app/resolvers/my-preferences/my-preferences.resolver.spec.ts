import { TestBed } from '@angular/core/testing';
import { ResolveFn } from '@angular/router';

import { myPreferencesResolver } from './my-preferences.resolver';

describe('myPreferencesResolver', () => {
  const executeResolver: ResolveFn<boolean> = (...resolverParameters) => 
      TestBed.runInInjectionContext(() => myPreferencesResolver(...resolverParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeResolver).toBeTruthy();
  });
});
