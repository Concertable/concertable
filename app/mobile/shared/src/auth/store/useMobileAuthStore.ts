import { create } from "zustand";
import type { User } from "@concertable/shared/features/auth/types";

interface MobileAuthState {
  user?: User;
  setUser: (user: User | undefined) => void;
}

export const useMobileAuthStore = create<MobileAuthState>()((set) => ({
  user: undefined,
  setUser: (user) => set({ user }),
}));
