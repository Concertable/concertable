import { useEffect, useState } from "react";
import { notificationConnection } from "@/lib/signalr";

interface Options {
  event: string;
  timeoutMs?: number;
}

export type CheckoutFlowState<TPayload> =
  | { phase: "awaiting" | "timeout" }
  | { phase: "success"; result: TPayload };

export function useCheckoutFlow<TPayload>({
  event,
  timeoutMs = 30_000,
}: Options): CheckoutFlowState<TPayload> {
  const [state, setState] = useState<CheckoutFlowState<TPayload>>({
    phase: "awaiting",
  });

  useEffect(() => {
    if (state.phase !== "awaiting") return;

    const handler = (payload: TPayload) => {
      setState({ phase: "success", result: payload });
    };

    notificationConnection.on(event, handler);
    const timeoutId = setTimeout(() => {
      setState((s) => (s.phase === "awaiting" ? { phase: "timeout" } : s));
    }, timeoutMs);

    return () => {
      notificationConnection.off(event, handler);
      clearTimeout(timeoutId);
    };
  }, [state.phase, event, timeoutMs]);

  return state;
}
