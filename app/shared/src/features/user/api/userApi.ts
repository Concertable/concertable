import { apiClient } from "../../../lib/apiClient";
import type { User } from "../../auth/types";

const userApi = {
  getMe: async (): Promise<User> => {
    const { data } = await apiClient.get<User>("/auth/me");
    return data;
  },

  updateLocation: async (
    latitude: number,
    longitude: number,
  ): Promise<User> => {
    const { data } = await apiClient.put<User>("/users/location", {
      latitude,
      longitude,
    });
    return data;
  },
};

export default userApi;
