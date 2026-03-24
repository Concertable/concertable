export interface Pagination<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}
