import api from "../../../lib/axiosClient";
import type { User } from "../../auth/types";

const userApi = {
  getMe: async (): Promise<User> => {
    const { data } = await api.get<User>("/auth/me");
    return data;
  },

  updateLocation: async (
    latitude: number,
    longitude: number,
  ): Promise<User> => {
    const { data } = await api.put<User>("/users/location", {
      latitude,
      longitude,
    });
    return data;
  },
};

export default userApi;
