import api from "@/lib/axios";
import type { Concert } from "@/types/concert";

interface UpdateConcertRequest {
  name: string;
  about: string;
  price: number;
  totalTickets: number;
}

export async function getConcert(id: number): Promise<Concert> {
  const { data } = await api.get<Concert>(`/concert/${id}`);
  return data;
}

export async function updateConcert(
  id: number,
  request: UpdateConcertRequest,
): Promise<Concert> {
  const { data } = await api.put<Concert>(`/concert/${id}`, request);
  return data;
}
