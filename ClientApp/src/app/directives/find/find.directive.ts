import { Directive, Input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HeaderType } from '../../models/header-type';
import { SearchParams } from '../../models/search-params';
import { Pagination } from '../../models/pagination';
import { SearchParamsSerializerServiceService } from '../../services/search-params-serializer/search-params-serializer-service.service';
import { PaginationHandler } from '../../shared/handler/pagination-handler';
import { PageEvent } from '@angular/material/paginator';
import { PaginationParams } from '../../models/pagination-params';

@Directive({
  selector: '[appFind]',
  standalone: false
})
export abstract class FindDirective<T> extends PaginationHandler<T> {
  @Input() headerType?: HeaderType;

  searchParams: Partial<SearchParams> = {
    headerType: 'event'
  };

  constructor(
    protected searchParamsSerializerService: SearchParamsSerializerServiceService,
    protected router: Router,
    protected route: ActivatedRoute
  ) {
    super();
  }

  changeHeaderType(headerType?: HeaderType) {
    this.searchParams.headerType = headerType;
  }

  updateSearchParams(updatedParams: Partial<SearchParams>) {
    Object.entries(updatedParams).forEach(([key, value]) => {
      if (value !== undefined) {
        (this.searchParams as any)[key] = value;
      }
    });
  }

  override get pageParams(): PaginationParams {
    return {
      pageNumber: this.searchParams.pageNumber ?? 1,
      pageSize: this.searchParams.pageSize ?? 5
    };
  }
  
  override set pageParams(params: PaginationParams) {
    this.searchParams.pageNumber = params.pageNumber;
    this.searchParams.pageSize = params.pageSize;
  }
}
