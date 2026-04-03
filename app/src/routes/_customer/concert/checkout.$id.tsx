import { createFileRoute } from "@tanstack/react-router";
import ConcertCheckoutPage from "@/pages/customer/concert/ConcertCheckoutPage";

export const Route = createFileRoute("/_customer/concert/checkout/$id")({
  params: {
    parse: (params) => ({ id: Number(params.id) }),
    stringify: (params) => ({ id: String(params.id) }),
  },
  component: ConcertCheckoutPage,
});
