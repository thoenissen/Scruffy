import { TestBed } from '@angular/core/testing';

import { ApiPipeService } from './api-pipe.service';

describe('ApiPipeService', () => {
  let service: ApiPipeService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ApiPipeService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
