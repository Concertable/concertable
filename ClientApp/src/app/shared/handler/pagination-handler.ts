import { PageEvent } from "@angular/material/paginator";
import { Pagination } from "../../models/pagination";
import { PaginationParams } from "../../models/pagination-params";
import { Subscription } from "rxjs";
import { Directive, OnDestroy } from "@angular/core";

@Directive()
export abstract class PaginationHandler<T> implements OnDestroy {
    protected subscriptions: Subscription[] = [];

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

    ngOnDestroy(): void {
      this.subscriptions.forEach(sub => sub.unsubscribe());
    }
  }
  