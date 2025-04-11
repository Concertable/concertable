import { PageEvent } from "@angular/material/paginator";
import { Pagination } from "../../models/pagination";
import { PaginationParams } from "../../models/pagination-params";

export abstract class PaginationHandler<T> {
    protected _pageParams: PaginationParams = {
        pageNumber: 1,
        pageSize: 5
      };
    
      get pageParams(): PaginationParams {
        return this._pageParams;
      }
    
      set pageParams(params: PaginationParams) {
        this._pageParams = params;
      }
  
    paginatedData?: Pagination<T>;
  
    abstract loadPage(): void;
  
    onPageChange(params: PaginationParams): void {
        this.pageParams = params;
        this.loadPage();
    }
  }
  