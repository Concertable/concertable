import { create } from "zustand";
import { persist } from "zustand/middleware";
import api from "@/lib/axios";
import type {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  User,
} from "@/types/auth";

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  login: (request: LoginRequest) => Promise<string>;
  register: (request: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  refresh: () => Promise<void>;
  updateUser: (user: User) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,

      login: async (request) => {
        const { data } = await api.post<LoginResponse>("/auth/login", request);
        set({
          user: data.user,
          accessToken: data.accessToken,
          refreshToken: data.refreshToken,
          isAuthenticated: true,
        });
        return data.user.baseUrl;
      },

      register: async (request) => {
        await api.post("/auth/register", request);
      },

      logout: async () => {
        const { refreshToken } = get();
        await api.post("/auth/logout", { refreshToken });
        set({
          user: null,
          accessToken: null,
          refreshToken: null,
          isAuthenticated: false,
        });
      },

      updateUser: (user) => set({ user }),

      refresh: async () => {
        const { refreshToken } = get();
        try {
          const { data } = await api.post<LoginResponse>("/auth/refresh", {
            refreshToken,
          });
          set({
            user: data.user,
            accessToken: data.accessToken,
            refreshToken: data.refreshToken,
            isAuthenticated: true,
          });
        } catch {
          set({
            user: null,
            accessToken: null,
            refreshToken: null,
            isAuthenticated: false,
          });
        }
      },
    }),
    {
      name: "auth",
      partialize: (state) => ({ refreshToken: state.refreshToken }),
    },
  ),
);
