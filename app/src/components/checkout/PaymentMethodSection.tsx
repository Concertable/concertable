import type { ComponentType } from "react";
import { ImmediatePaymentSection } from "@/components/checkout/ImmediatePaymentSection";
import { DeferredPaymentSection } from "@/components/checkout/DeferredPaymentSection";
import type { PaymentMethod } from "@/api/stripeAccountApi";
import type { PaymentTiming } from "@/types/acceptCheckout";

export interface PaymentSectionProps {
  savedCard: PaymentMethod | null | undefined;
  isLoading: boolean;
  onChange: (paymentMethodId: string | null | undefined) => void;
}

const sectionRegistry = {
  immediate: ImmediatePaymentSection,
  deferred: DeferredPaymentSection,
} as const satisfies Record<PaymentTiming, ComponentType<PaymentSectionProps>>;

export function PaymentMethodSection({
  timing,
  ...props
}: PaymentSectionProps & { timing: PaymentTiming }) {
  const Section = sectionRegistry[timing];
  return <Section {...props} />;
}
