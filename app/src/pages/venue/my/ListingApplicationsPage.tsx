import { useParams } from "@tanstack/react-router";
import { useApplicationsByOpportunityQuery } from "@/hooks/query/useApplicationQuery";
import { ApplicationCard } from "@/components/applications/ApplicationCard";

export default function ListingApplicationsPage() {
  const { id } = useParams({ from: "/venue/my/applications/$id" });
  const { data: applications, isLoading } = useApplicationsByOpportunityQuery(Number(id));

  if (isLoading) return null;

  return (
    <div className="p-6 max-w-3xl mx-auto space-y-4">
      <h1 className="text-xl font-semibold">Applications</h1>
      {applications?.length === 0 && (
        <p className="text-sm text-muted-foreground">No applications yet.</p>
      )}
      {applications?.map((application) => (
        <ApplicationCard key={application.id} application={application} />
      ))}
    </div>
  );
}
