import { createFileRoute } from "@tanstack/react-router";
import { MyConcertPage } from "@/features/concerts";

export const Route = createFileRoute("/artist/my/concerts/concert/$id")({
  params: {
    parse: (params) => ({ id: Number(params.id) }),
    stringify: (params) => ({ id: String(params.id) }),
  },
  component: () => {
    const { id } = Route.useParams();
    return <MyConcertPage id={id} />;
  },
});
