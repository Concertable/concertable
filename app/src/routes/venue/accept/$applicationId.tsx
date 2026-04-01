import { createFileRoute } from "@tanstack/react-router";
import AcceptApplicationPage from "@/pages/venue/accept/AcceptApplicationPage";

export const Route = createFileRoute("/venue/accept/$applicationId")({
  params: {
    parse: (params) => ({ applicationId: Number(params.applicationId) }),
    stringify: (params) => ({ applicationId: String(params.applicationId) }),
  },
  component: AcceptApplicationPage,
});
