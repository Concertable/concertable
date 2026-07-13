import api from "../../../lib/axiosClient";
import type { Concert } from "../types";
import type { UpdateConcertRequest } from "../schemas/updateConcertRequestSchema";

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

  getAgreementPdf: async (id: number): Promise<Blob> => {
    const { data } = await api.get<ArrayBuffer>(
      `/concert/${id}/agreement/pdf`,
      { responseType: "arraybuffer" },
    );
    return new Blob([data], { type: "application/pdf" });
  },
};

export default concertApi;
