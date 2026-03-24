import { createFileRoute } from "@tanstack/react-router";
import ConcertPage from "@/pages/artist/my/concerts/ConcertPage";

export const Route = createFileRoute("/artist/my/concerts/concert/$id")({
  component: ConcertPage,
});
