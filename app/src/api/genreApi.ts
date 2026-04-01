import api from "@/lib/axios";
import type { Genre } from "@/types/common";

export async function getAll(): Promise<Genre[]> {
  const { data } = await api.get<Genre[]>("/genre/all");
  return data;
}
