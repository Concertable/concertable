import { useCheckoutFlow as useCheckoutFlowBase } from "@concertable/shared/features/concerts";
import { notificationConnection } from "@/lib/signalr";

export type { CheckoutFlowState } from "@concertable/shared/features/concerts";

interface Options<TPayload, TFailure> {
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

export function useCheckoutFlow<TPayload, TFailure = never>({
  event,
  failureEvent,
  active,
  timeoutActive,
  timeoutMs,
  matchesSuccess,
  matchesFailure,
  onFailure,
  resetKey,
}: Readonly<Options<TPayload, TFailure>>) {
  return useCheckoutFlowBase<TPayload, TFailure>({
    connection: notificationConnection,
    event,
    failureEvent,
    active,
    timeoutActive,
    timeoutMs,
    matchesSuccess,
    matchesFailure,
    onFailure,
    resetKey,
  });
}
