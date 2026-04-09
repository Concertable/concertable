import api from "@/lib/axios";
import type { User } from "@/types/auth";

const userApi = {
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
