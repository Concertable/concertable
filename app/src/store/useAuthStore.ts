import { create } from "zustand";
import { persist } from "zustand/middleware";
import api from "@/lib/axios";
import type { LoginRequest, LoginResponse, RegisterRequest, User } from "@/types/auth";

interface AuthState {
  user: User | null;
  baseUrl: string | null;
  accessToken: string | null;
  refreshToken: string | null;
  login: (request: LoginRequest) => Promise<void>;
  register: (request: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  refresh: () => Promise<void>;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      baseUrl: null,
      accessToken: null,
      refreshToken: null,

      login: async (request) => {
        const { data } = await api.post<LoginResponse>("/auth/login", request);
        set({ user: data.user, baseUrl: data.baseUrl, accessToken: data.accessToken, refreshToken: data.refreshToken });
      },

      register: async (request) => {
        await api.post("/auth/register", request);
      },

      logout: async () => {
        const { refreshToken } = get();
        await api.post("/auth/logout", { refreshToken });
        set({ user: null, baseUrl: null, accessToken: null, refreshToken: null });
      },

      refresh: async () => {
        const { refreshToken } = get();
        try {
          const { data } = await api.post<LoginResponse>("/auth/refresh", { refreshToken });
          set({ user: data.user, baseUrl: data.baseUrl, accessToken: data.accessToken, refreshToken: data.refreshToken });
        } catch {
          set({ user: null, baseUrl: null, accessToken: null, refreshToken: null });
        }
      },
    }),
    {
      name: "auth",
      partialize: (state) => ({ refreshToken: state.refreshToken }),
    }
  )
);
