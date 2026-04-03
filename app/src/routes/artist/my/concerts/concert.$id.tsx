import { createFileRoute } from "@tanstack/react-router";
import ConcertPage from "@/pages/artist/my/concerts/ConcertPage";

export const Route = createFileRoute("/artist/my/concerts/concert/$id")({
  params: {
    parse: (params) => ({ id: Number(params.id) }),
    stringify: (params) => ({ id: String(params.id) }),
  },
  component: ConcertPage,
});
