import { create } from "zustand";
import { persist } from "zustand/middleware";
import api from "@/lib/axios";
import type { LoginRequest, LoginResponse, RegisterRequest, User } from "@/types/auth";

interface AuthState {
  user: User | undefined;
  accessToken: string | undefined;
  refreshToken: string | undefined;
  isLoading: boolean;
  login: (request: LoginRequest) => Promise<void>;
  register: (request: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  refresh: () => Promise<void>;
  getMe: () => Promise<void>;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: undefined,
      accessToken: undefined,
      refreshToken: undefined,
      isLoading: false,

      login: async (request) => {
        const { data } = await api.post<LoginResponse>("/account/login", request);
        set({ user: data.user, accessToken: data.accessToken, refreshToken: data.refreshToken });
      },

      register: async (request) => {
        await api.post("/account/register", request);
      },

      logout: async () => {
        const { refreshToken } = get();
        await api.post("/account/logout", { refreshToken });
        set({ user: undefined, accessToken: undefined, refreshToken: undefined });
      },

      refresh: async () => {
        const { refreshToken } = get();
        const { data } = await api.post<LoginResponse>("/account/refresh", { refreshToken });
        set({ user: data.user, accessToken: data.accessToken, refreshToken: data.refreshToken });
      },

      getMe: async () => {
        set({ isLoading: true });
        try {
          const { data } = await api.get<User>("/account/me");
          set({ user: data });
        } catch {
          set({ user: undefined });
        } finally {
          set({ isLoading: false });
        }
      },
    }),
    {
      name: "auth",
      partialize: (state) => ({ refreshToken: state.refreshToken }),
    }
  )
);
