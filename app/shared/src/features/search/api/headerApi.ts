import { searchClient } from "../../../lib/searchClient";
import type { Pagination } from "../../../types/common";
import type { Header, HeaderType, SortToken } from "../types";
import type { SearchFilters } from "../schemas/searchSchema";

interface HeaderSearchParams {
  searchTerm?: string;
  headerType: HeaderType;
  latitude?: number;
  longitude?: number;
  from?: string;
  to?: string;
  genres?: SearchFilters["genres"];
  radiusKm?: number;
  sort?: SortToken;
  showHistory?: boolean;
  showSold?: boolean;
}

function toSearchParams(filters: SearchFilters): HeaderSearchParams {
  return {
    searchTerm: filters.query,
    headerType: filters.headerType,
    latitude: filters.lat,
    longitude: filters.lng,
    from: filters.from,
    to: filters.to,
    genres: filters.genres,
    radiusKm: filters.radius,
    sort: filters.orderBy
      ? (`${filters.orderBy}_${filters.sortOrder ?? "asc"}` satisfies SortToken)
      : undefined,
    showHistory: filters.showHistory,
    showSold: filters.showSold,
  };
}

const headerApi = {
  getByAmount: async (
    amount: number,
    headerType: HeaderType,
  ): Promise<Header[]> => {
    const { data } = await searchClient.get<Header[]>(`/header/amount/${amount}`, {
      params: { headerType },
    });
    return data;
  },

  searchHeaders: async (
    filters: SearchFilters,
  ): Promise<Pagination<Header>> => {
    const { data } = await searchClient.get<Pagination<Header>>("/header", {
      params: toSearchParams(filters),
    });
    return data;
  },
};

export default headerApi;
