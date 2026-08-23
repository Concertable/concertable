import type { User } from "@concertable/shared/features/auth";
import { useMobileAuthStore } from "./store/useMobileAuthStore";

export const mobileAuthSession = {
  current(): User | undefined {
    return useMobileAuthStore.getState().user;
  },
  set(user: User): void {
    useMobileAuthStore.getState().setUser(user);
  },
  clear(): void {
    useMobileAuthStore.getState().setUser(undefined);
  },
  subscribe(
    listener: (user: User | undefined, previousUser: User | undefined) => void,
  ): () => void {
    return useMobileAuthStore.subscribe((state, previousState) =>
      listener(state.user, previousState.user),
    );
  },
};
