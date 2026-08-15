import { useEffect, useState } from "react";
import type { HubConnection } from "@microsoft/signalr";

interface Options {
  connection: HubConnection;
  event: string;
  timeoutMs?: number;
}

export type CheckoutFlowState<TPayload> =
  | { phase: "awaiting" | "timeout" }
  | { phase: "success"; result: TPayload };

export function useCheckoutFlow<TPayload>({
  connection,
  event,
  timeoutMs = 30_000,
}: Readonly<Options>): CheckoutFlowState<TPayload> {
  const [state, setState] = useState<CheckoutFlowState<TPayload>>({
    phase: "awaiting",
  });

  useEffect(() => {
    if (state.phase !== "awaiting") return;

    const handler = (payload: TPayload) => {
      setState({ phase: "success", result: payload });
    };

    connection.on(event, handler);
    const timeoutId = setTimeout(() => {
      setState((s) => (s.phase === "awaiting" ? { phase: "timeout" } : s));
    }, timeoutMs);

    return () => {
      connection.off(event, handler);
      clearTimeout(timeoutId);
    };
  }, [state.phase, connection, event, timeoutMs]);

  return state;
}
