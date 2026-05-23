import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Pagination } from '../../models/pagination';
import { PageEvent } from '@angular/material/paginator';
import { PaginationParams } from '../../models/pagination-params';

@Component({
  selector: 'app-paginator',
  standalone: false,
  templateUrl: './paginator.component.html',
  styleUrl: './paginator.component.scss'
})
export class PaginatorComponent<T> {
  @Input() pagination?: Pagination<T>;
  @Input() pageSizeOptions: number[] = [];
  @Output() pageChange = new EventEmitter<PaginationParams>();

  get pageIndex(): number {
    return (this.pagination?.pageNumber ?? 1) - 1;
  }

  onPageChange(event: PageEvent): void {
    this.pageChange.emit({
      pageNumber: event.pageIndex + 1,
      pageSize: event.pageSize
    });
  }
}
