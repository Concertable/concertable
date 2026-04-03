import { createFileRoute } from "@tanstack/react-router";
import OpportunityApplicationsPage from "@/pages/venue/my/OpportunityApplicationsPage";

export const Route = createFileRoute("/venue/my/applications/$id")({
  params: {
    parse: (params) => ({ id: Number(params.id) }),
    stringify: (params) => ({ id: String(params.id) }),
  },
  component: OpportunityApplicationsPage,
});
