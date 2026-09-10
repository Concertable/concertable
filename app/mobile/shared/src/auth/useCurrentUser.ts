import { useMobileAuthStore } from "./store/useMobileAuthStore";

export function useCurrentUser() {
  return useMobileAuthStore((state) => state.user);
}
