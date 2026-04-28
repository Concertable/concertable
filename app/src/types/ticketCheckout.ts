import type { CheckoutSession } from "@/types/checkoutSession";

export interface TicketCheckout {
  session: CheckoutSession;
  price: number;
  concertId: number;
}
