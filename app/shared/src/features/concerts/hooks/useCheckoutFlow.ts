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
    console.log("[useCheckoutFlow] effect", {
      phase: state.phase,
      event,
      connectionState: connection.state,
    });
    if (state.phase !== "awaiting") return;

    const handler = (payload: TPayload) => {
      console.log("[useCheckoutFlow] event fired", { event, payload });
      setState({ phase: "success", result: payload });
    };

    connection.on(event, handler);
    console.log("[useCheckoutFlow] subscribed", {
      event,
      connectionState: connection.state,
    });
    const timeoutId = setTimeout(() => {
      console.warn("[useCheckoutFlow] timeout reached", { event });
      setState((s) => (s.phase === "awaiting" ? { phase: "timeout" } : s));
    }, timeoutMs);

    return () => {
      console.log("[useCheckoutFlow] cleanup — unsubscribing", {
        event,
        connectionState: connection.state,
      });
      connection.off(event, handler);
      clearTimeout(timeoutId);
    };
  }, [state.phase, connection, event, timeoutMs]);

  return state;
}
