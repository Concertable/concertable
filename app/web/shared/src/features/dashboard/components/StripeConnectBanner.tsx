import { Link } from "@tanstack/react-router";
import { AlertTriangle } from "lucide-react";
import type { StripeConnectStatus } from "@concertable/shared/features/dashboard";

export function StripeConnectBanner({ status }: { status: StripeConnectStatus }) {
  if (status.state === "Complete") return null;

  return (
    <div className="flex items-start gap-3 rounded-lg border border-amber-200 bg-amber-50 p-4">
      <AlertTriangle className="mt-0.5 size-5 shrink-0 text-amber-600" />
      <div className="flex-1">
        <p className="text-sm font-semibold text-amber-900">
          Finish Stripe setup to receive payouts
        </p>
        <p className="text-xs text-amber-800">
          Without this, settlements won't reach your bank.
        </p>
      </div>
      <Link
        to={status.href}
        className="rounded-md bg-amber-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-amber-700"
      >
        Complete setup →
      </Link>
    </div>
  );
}
