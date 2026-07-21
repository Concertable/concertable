import { apiClient } from "../../../lib/apiClient";
import type { Genre } from "../../../types/common";

const genreApi = {
  getAll: async (): Promise<Genre[]> => {
    const { data } = await apiClient.get<Genre[]>("/genre");
    return data;
  },
};

export default genreApi;
