import type { Contract } from "@/features/contracts";

export type PaymentTiming = "immediate" | "deferred";

export function resolvePaymentTiming(contract: Contract): PaymentTiming {
  return contract.$type === "flatFee" || contract.$type === "venueHire"
    ? "immediate"
    : "deferred";
}
