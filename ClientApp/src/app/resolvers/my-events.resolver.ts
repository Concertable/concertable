import { ResolveFn } from '@angular/router';

export const myEventsResolver: ResolveFn<boolean> = (route, state) => {
  return true;
};
