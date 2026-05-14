import { useState } from "react";

export interface PaginationParams {
  pageNumber: number;
  pageSize: number;
}

export interface UsePaginationResult {
  params: PaginationParams;
  setPage: (page: number) => void;
  nextPage: () => void;
  prevPage: () => void;
}

export function usePagination(pageSize = 5): UsePaginationResult {
  const [pageNumber, setPageNumber] = useState(1);

  return {
    params: { pageNumber, pageSize },
    setPage: setPageNumber,
    nextPage: () => setPageNumber((n) => n + 1),
    prevPage: () => setPageNumber((n) => Math.max(1, n - 1)),
  };
}
