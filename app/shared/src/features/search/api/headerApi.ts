import api from "../../../lib/searchAxiosClient";
import type { Pagination } from "../../../types/common";
import type { Header, HeaderType } from "../types";
import type { SearchFilters } from "../schemas/searchSchema";

const headerApi = {
  getByAmount: async (
    amount: number,
    headerType: HeaderType,
  ): Promise<Header[]> => {
    const { data } = await api.get<Header[]>(`/header/amount/${amount}`, {
      params: { headerType },
    });
    return data;
  },

  searchHeaders: async (
    filters: SearchFilters,
  ): Promise<Pagination<Header>> => {
    const params = {
      searchTerm: filters.query,
      headerType: filters.headerType,
      latitude: filters.lat,
      longitude: filters.lng,
      from: filters.from,
      to: filters.to,
      genres: filters.genres,
      radiusKm: filters.radius,
      sort:
        filters.orderBy && filters.sortOrder
          ? `${filters.orderBy}_${filters.sortOrder}`
          : undefined,
      showHistory: filters.showHistory,
      showSold: filters.showSold,
    };
    const { data } = await api.get<Pagination<Header>>("/header", { params });
    return data;
  },
};

export default headerApi;
