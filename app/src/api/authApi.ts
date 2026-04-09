import api from "@/lib/axios";

const authApi = {
  sendVerificationEmail: async (email: string): Promise<void> => {
    await api.post("/auth/send-verification", { email });
  },

  changePassword: async (
    currentPassword: string,
    newPassword: string,
  ): Promise<void> => {
    await api.post("/auth/change-password", { currentPassword, newPassword });
  },
};

export default authApi;
