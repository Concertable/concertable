import { TestBed } from '@angular/core/testing';

import { EventToastService } from './event-toast.service';

describe('EventToastService', () => {
  let service: EventToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EventToastService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
