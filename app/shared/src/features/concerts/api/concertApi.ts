import { apiClient } from "../../../lib/apiClient";
import type { Concert, UpdateConcertRequest } from "../types";
import type { DoorRevenueRequest } from "../schemas/doorRevenueRequestSchema";

const concertApi = {
  getConcert: async (id: number): Promise<Concert> => {
    const { data } = await apiClient.get<Concert>(`/concert/${id}`);
    return data;
  },

  updateConcert: async (
    id: number,
    request: UpdateConcertRequest,
  ): Promise<Concert> => {
    const { data } = await apiClient.put<Concert>(`/concert/${id}`, request);
    return data;
  },

  cancelConcert: async (id: number): Promise<void> => {
    await apiClient.post(`/concert/${id}/cancel`);
  },

  declareDoorRevenue: async (
    id: number,
    request: DoorRevenueRequest,
  ): Promise<void> => {
    await apiClient.post(`/concert/${id}/door-revenue`, request);
  },

  getContractPdf: async (id: number): Promise<Blob> => {
    const { data } = await apiClient.get<ArrayBuffer>(
      `/concert/${id}/contract/pdf`,
      { responseType: "arraybuffer" },
    );
    return new Blob([data], { type: "application/pdf" });
  },
};

export default concertApi;
