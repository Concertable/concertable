import type { ComponentType } from "react";
import type { PaymentMethod } from "@/features/payments";
import { ImmediatePaymentSection } from "./ImmediatePaymentSection";
import { DeferredPaymentSection } from "./DeferredPaymentSection";
import type { PaymentTiming } from "../../types";

export interface PaymentSectionProps {
  savedCard: PaymentMethod | null | undefined;
  isLoading: boolean;
  onChange: (paymentMethodId: string | null | undefined) => void;
}

const sectionRegistry = {
  Immediate: ImmediatePaymentSection,
  Deferred: DeferredPaymentSection,
} as const satisfies Record<PaymentTiming, ComponentType<PaymentSectionProps>>;

export function PaymentMethodSection({
  timing,
  ...props
}: PaymentSectionProps & { timing: PaymentTiming }) {
  const Section = sectionRegistry[timing];
  return <Section {...props} />;
}
