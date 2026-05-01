import api from "@/lib/axios";
import type { User } from "@/types/auth";

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
