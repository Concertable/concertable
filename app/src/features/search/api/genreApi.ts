import api from "@/lib/axios";
import type { Genre } from "@/types/common";

const genreApi = {
  getAll: async (): Promise<Genre[]> => {
    const { data } = await api.get<Genre[]>("/genre/all");
    return data;
  },
};

export default genreApi;
