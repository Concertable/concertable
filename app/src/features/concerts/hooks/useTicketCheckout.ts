import { useState, useEffect } from "react";
import { notificationConnection } from "@/lib/signalr";
import type { TicketPurchasedPayload } from "@/features/notifications";
import ticketApi from "../api/ticketApi";
import { handle3ds } from "../utils/handle3ds";

export type TicketCheckoutState =
  | "idle"
  | "processing"
  | "pending"
  | "success"
  | "error";

export function useTicketCheckout(concertId: number) {
  const [quantity, setQuantity] = useState(1);
  const [paymentMethodId, setPaymentMethodId] = useState<string | undefined>(
    undefined,
  );
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
    if (!paymentMethodId) {
      setError("Please select a payment method.");
      setState("error");
      return;
    }

    setState("processing");
    setError(null);

    try {
      const response = await ticketApi.purchase({
        concertId,
        quantity,
        paymentMethodId,
      });

      await handle3ds(response);
      setState("pending");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Payment failed. Please try again.");
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
