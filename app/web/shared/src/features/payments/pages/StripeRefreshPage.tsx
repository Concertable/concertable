import { useMountEffect } from "@concertable/shared/hooks/useMountEffect";

export function StripeRefreshPage() {
  useMountEffect(() => {
    window.opener?.postMessage(
      { type: "stripe_refresh" },
      window.location.origin,
    );
    window.close();
  });

  return null;
}
