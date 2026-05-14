import api from "@/lib/axios";
import type { Concert } from "../types";

interface UpdateConcertRequest {
  name: string;
  about: string;
  price: number;
  totalTickets: number;
}

const concertApi = {
  getConcert: async (id: number): Promise<Concert> => {
    const { data } = await api.get<Concert>(`/concert/${id}`);
    return data;
  },

  updateConcert: async (
    id: number,
    request: UpdateConcertRequest,
  ): Promise<Concert> => {
    const { data } = await api.put<Concert>(`/concert/${id}`, request);
    return data;
  },
};

export default concertApi;
