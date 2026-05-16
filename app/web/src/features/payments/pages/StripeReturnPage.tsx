import { useRef } from "react";
import { useMountEffect } from "@concertable/shared/hooks/useMountEffect";

export function StripeReturnPage() {
  const sent = useRef(false);

  useMountEffect(() => {
    if (sent.current) return;
    sent.current = true;
    window.opener?.postMessage(
      { type: "stripe_return" },
      window.location.origin,
    );
    window.close();
  });

  return null;
}
