import { useMountEffect } from "@/hooks/useMountEffect";

export default function StripeReturnPage() {
  useMountEffect(() => {
    window.opener?.postMessage(
      { type: "stripe_return" },
      window.location.origin,
    );
    window.close();
  });

  return null;
}
