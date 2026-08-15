import { useEffect, useState } from "react";
import type { HubConnection } from "@microsoft/signalr";

interface Options<TPayload, TFailure> {
  connection: HubConnection;
  event: string;
  failureEvent?: string;
  active: boolean;
  timeoutActive?: boolean;
  timeoutMs?: number;
  matchesSuccess?: (payload: TPayload) => boolean;
  matchesFailure?: (failure: TFailure) => boolean;
  onFailure?: (failure: TFailure) => void;
  resetKey?: unknown;
}

export type CheckoutFlowState<TPayload, TFailure = never> =
  | { phase: "awaiting" | "timeout" }
  | { phase: "success"; result: TPayload }
  | { phase: "failure"; failure: TFailure };

export function useCheckoutFlow<TPayload, TFailure = never>({
  connection,
  event,
  failureEvent,
  active,
  timeoutActive = active,
  timeoutMs = 30_000,
  matchesSuccess,
  matchesFailure,
  onFailure,
  resetKey,
}: Readonly<Options<TPayload, TFailure>>): CheckoutFlowState<
  TPayload,
  TFailure
> {
  const [state, setState] = useState<CheckoutFlowState<TPayload, TFailure>>({
    phase: "awaiting",
  });

  useEffect(() => {
    setState((current) =>
      current.phase === "awaiting" ? current : { phase: "awaiting" },
    );
  }, [active, resetKey]);

  useEffect(() => {
    if (!active) return;

    const successHandler = (payload: TPayload) => {
      if (matchesSuccess && !matchesSuccess(payload)) return;
      setState({ phase: "success", result: payload });
    };
    const failureHandler = (failure: TFailure) => {
      if (matchesFailure && !matchesFailure(failure)) return;
      onFailure?.(failure);
      setState({ phase: "failure", failure });
    };

    connection.on(event, successHandler);
    if (failureEvent) connection.on(failureEvent, failureHandler);
    return () => {
      connection.off(event, successHandler);
      if (failureEvent) connection.off(failureEvent, failureHandler);
    };
  }, [
    active,
    connection,
    event,
    failureEvent,
    matchesFailure,
    matchesSuccess,
    onFailure,
  ]);

  useEffect(() => {
    if (!active || !timeoutActive || state.phase !== "awaiting") return;

    const timeoutId = setTimeout(() => {
      setState({ phase: "timeout" });
    }, timeoutMs);

    return () => clearTimeout(timeoutId);
  }, [active, state.phase, timeoutActive, timeoutMs]);

  return state;
}
