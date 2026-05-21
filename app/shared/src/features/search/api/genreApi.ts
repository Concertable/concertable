import api from "../../../lib/axiosClient";
import type { Genre } from "../../../types/common";

const genreApi = {
  getAll: async (): Promise<Genre[]> => {
    const { data } = await api.get<Genre[]>("/genre");
    return data;
  },
};

export default genreApi;
