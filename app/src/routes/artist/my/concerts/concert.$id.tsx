import { createFileRoute } from "@tanstack/react-router";
import { ArtistConcertPage } from "@/features/concerts";

export const Route = createFileRoute("/artist/my/concerts/concert/$id")({
  params: {
    parse: (params) => ({ id: Number(params.id) }),
    stringify: (params) => ({ id: String(params.id) }),
  },
  component: ArtistConcertPage,
});
