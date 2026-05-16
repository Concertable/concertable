import { useCheckoutFlow as useCheckoutFlowBase } from "@concertable/shared/features/concerts";
import { notificationConnection } from "@/lib/signalr";

export type { CheckoutFlowState } from "@concertable/shared/features/concerts";

interface Options {
  event: string;
  timeoutMs?: number;
}

export function useCheckoutFlow<TPayload>({ event, timeoutMs }: Readonly<Options>) {
  return useCheckoutFlowBase<TPayload>({ connection: notificationConnection, event, timeoutMs });
}
