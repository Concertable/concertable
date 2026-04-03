import { createFileRoute } from "@tanstack/react-router";
import ConcertDetailsPage from "@/pages/customer/find/ConcertDetailsPage";

export const Route = createFileRoute("/_customer/find/concert/$id")({
  params: {
    parse: (params) => ({ id: Number(params.id) }),
    stringify: (params) => ({ id: String(params.id) }),
  },
  component: ConcertDetailsPage,
});
