import { createFileRoute } from "@tanstack/react-router";
import ConcertPage from "@/pages/venue/my/concerts/ConcertPage";

export const Route = createFileRoute("/venue/my/concerts/concert/$id")({
  params: {
    parse: (params) => ({ id: Number(params.id) }),
    stringify: (params) => ({ id: String(params.id) }),
  },
  component: ConcertPage,
});
