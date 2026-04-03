export interface Pagination<T> {
    data: T[];
    totalCount: number;
    totalPages: number;
    pageNumber: number;
    pageSize: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
  }
  