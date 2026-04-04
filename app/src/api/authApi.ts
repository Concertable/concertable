import api from "@/lib/axios";

export async function sendVerificationEmail(email: string) {
  await api.post("/auth/send-verification", { email });
}

export async function changePassword(
  currentPassword: string,
  newPassword: string,
) {
  await api.post("/auth/change-password", { currentPassword, newPassword });
}
