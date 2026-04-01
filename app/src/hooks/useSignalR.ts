import { useEffect } from "react";
import { notificationConnection } from "@/lib/signalr";
import { useAuthStore } from "@/store/useAuthStore";

export function useSignalR() {
  const accessToken = useAuthStore((s) => s.accessToken);

  useEffect(() => {
    if (!accessToken) return;

    notificationConnection.start().catch(console.error);

    return () => {
      notificationConnection.stop().catch(console.error);
    };
  }, [accessToken]);
}
