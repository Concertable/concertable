import type { ReactNode } from "react";
import { CreditCard } from "lucide-react";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { usePaymentMethodQuery } from "../hooks/useStripeAccountQuery";
import { AddPaymentMethodModal } from "../components/AddPaymentMethodModal";

interface Props {
  payoutSlot?: ReactNode;
}

export function PaymentPage({ payoutSlot }: Readonly<Props>) {
  const { data: paymentMethod, isLoading: isPaymentMethodLoading } =
    usePaymentMethodQuery();

  return (
    <div className="max-w-lg space-y-8">
      <div>
        <h2 className="text-lg font-semibold">Payment & Billing</h2>
        <p className="text-muted-foreground text-sm">
          Manage your payment methods and billing details
        </p>
      </div>

      <Separator />

      <div className="space-y-4">
        <h3 className="font-medium">Payment Method</h3>
        {isPaymentMethodLoading ? (
          <Skeleton className="h-[66px] w-full rounded-lg" />
        ) : paymentMethod ? (
          <div className="flex items-center justify-between rounded-lg border p-4">
            <div className="flex items-center gap-3">
              <CreditCard className="text-muted-foreground size-5" />
              <div>
                <p className="text-sm font-medium capitalize">
                  {paymentMethod.brand} •••• {paymentMethod.last4}
                </p>
                <p className="text-muted-foreground text-xs">
                  Expires {paymentMethod.expMonth}/{paymentMethod.expYear}
                </p>
              </div>
            </div>
            <AddPaymentMethodModal replace />
          </div>
        ) : (
          <>
            <p className="text-muted-foreground text-sm">
              Save a card to check out without entering your details each time.
            </p>
            <div className="pt-2">
              <AddPaymentMethodModal />
            </div>
          </>
        )}
      </div>

      {payoutSlot}
    </div>
  );
}
