import { useState, useEffect } from "react";
import { loadStripe } from "@stripe/stripe-js";
import { notificationConnection } from "@/lib/signalr";
import type { TicketPurchasedPayload } from "@/features/notifications";
import ticketApi from "@/api/ticketApi";

const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY);

export type TicketCheckoutState =
  | "idle"
  | "processing"
  | "pending"
  | "success"
  | "error";

export function useTicketCheckout(concertId: number) {
  const [quantity, setQuantity] = useState(1);
  const [paymentMethodId, setPaymentMethodId] = useState<
    string | null | undefined
  >(undefined);
  const [state, setState] = useState<TicketCheckoutState>("idle");
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<TicketPurchasedPayload | null>(null);

  useEffect(() => {
    function handleTicketPurchased(payload: TicketPurchasedPayload) {
      if (payload.concertId !== concertId) return;
      if (payload.success) {
        setResult(payload);
        setState("success");
      } else {
        setError(payload.message);
        setState("error");
      }
    }

    notificationConnection.on("TicketPurchased", handleTicketPurchased);
    return () =>
      notificationConnection.off("TicketPurchased", handleTicketPurchased);
  }, [concertId]);

  async function purchase() {
    setState("processing");
    setError(null);

    try {
      const response = await ticketApi.purchase({
        concertId,
        quantity,
        paymentMethodId: paymentMethodId ?? undefined,
      });

      if (response.requiresAction && response.clientSecret) {
        const stripe = await stripePromise;
        if (!stripe) {
          setError("Stripe failed to load. Please refresh and try again.");
          setState("error");
          return;
        }
        const { error: stripeError } = await stripe.handleNextAction({
          clientSecret: response.clientSecret,
        });
        if (stripeError) {
          setError(stripeError.message ?? "Payment authentication failed.");
          setState("error");
          return;
        }
      }

      setState("pending");
    } catch {
      setError("Payment failed. Please try again.");
      setState("error");
    }
  }

  return {
    quantity,
    setQuantity,
    paymentMethodId,
    setPaymentMethodId,
    state,
    error,
    result,
    purchase,
  };
}
