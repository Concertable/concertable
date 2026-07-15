import api from "../../../lib/axiosClient";
import type { Concert } from "../types";
import type { UpdateConcertRequest } from "../schemas/updateConcertRequestSchema";
import type { DoorRevenueRequest } from "../schemas/doorRevenueRequestSchema";

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

  cancelConcert: async (id: number): Promise<void> => {
    await api.post(`/concert/${id}/cancel`);
  },

  declareDoorRevenue: async (
    id: number,
    request: DoorRevenueRequest,
  ): Promise<void> => {
    await api.post(`/concert/${id}/door-revenue`, request);
  },

  getContractPdf: async (id: number): Promise<Blob> => {
    const { data } = await api.get<ArrayBuffer>(
      `/concert/${id}/contract/pdf`,
      { responseType: "arraybuffer" },
    );
    return new Blob([data], { type: "application/pdf" });
  },
};

export default concertApi;
