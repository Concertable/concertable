import { Directive, Input } from '@angular/core';
import { HeaderType } from '../../models/header-type';
import { SearchParams } from '../../models/search-params';
import { ActivatedRoute, Router, UrlSerializer } from '@angular/router';
import { SearchParamsSerializerServiceService } from '../../services/search-params-serializer/search-params-serializer-service.service';

@Directive({
  selector: '[appFind]',
  standalone: false
})
export abstract class FindDirective {
  constructor(
    protected searchParamsSerializerService: SearchParamsSerializerServiceService,
    protected router: Router,
    protected route: ActivatedRoute
  ) { }

  @Input() headerType?: HeaderType;

  searchParams: Partial<SearchParams> = {
    headerType: 'event'
  }

  abstract handleSearch(): void;

  changeHeaderType(headerType?: HeaderType) {
    this.searchParams.headerType = headerType;
  }

  updateSearchParams(updatedParams: Partial<SearchParams>) {
    Object.entries(updatedParams).forEach(([key, value]) => {
      if (value !== undefined) {
        (this.searchParams as any)[key] = value;
      }
    });
    console.log("Updated SearchParams:", this.searchParams);
  }
}
