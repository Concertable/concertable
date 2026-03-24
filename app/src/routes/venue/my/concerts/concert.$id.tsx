import { createFileRoute } from "@tanstack/react-router";
import ConcertPage from "@/pages/venue/my/concerts/ConcertPage";

export const Route = createFileRoute("/venue/my/concerts/concert/$id")({
  component: ConcertPage,
});
