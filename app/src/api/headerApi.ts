import api from "@/lib/axios";
import type { SearchFilters } from "@/components/SearchBar";
import type { Header } from "@/types/header";
import type { Pagination } from "@/types/common";

export async function searchHeaders(filters: SearchFilters): Promise<Pagination<Header>> {
  const { data } = await api.get<Pagination<Header>>("/header", { params: filters });
  return data;
}
