import type { ConcertApplication } from "@/types/application";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { useNavigate } from "@tanstack/react-router";
import dayjs from "dayjs";

interface Props {
  application: ConcertApplication;
  onDeny?: (applicationId: number) => void;
}

export function ApplicationCard({ application, onDeny }: Readonly<Props>) {
  const navigate = useNavigate();
  const { artist, opportunity, status } = application;

  function handleAccept() {
    navigate({ to: "/venue/accept/$applicationId", params: { applicationId: application.id } });
  }

  return (
    <div className="rounded-xl border border-border bg-card p-4 space-y-3">
      <div className="flex items-start justify-between gap-4">
        <div className="flex items-center gap-3">
          {artist.imageUrl && (
            <img src={artist.imageUrl} alt={artist.name} className="w-10 h-10 rounded-full object-cover" />
          )}
          <div className="space-y-0.5">
            <p className="font-medium">{artist.name}</p>
            <p className="text-sm text-muted-foreground">
              {dayjs(opportunity.startDate).format("D MMM YYYY")} — {dayjs(opportunity.endDate).format("D MMM YYYY")}
            </p>
          </div>
        </div>

        <div className="flex items-center gap-2 shrink-0">
          <Badge variant="outline">{status}</Badge>
          {status === "Pending" && (
            <>
              <Button size="sm" onClick={handleAccept}>Accept</Button>
              {onDeny && (
                <Button size="sm" variant="destructive" onClick={() => onDeny(application.id)}>
                  Deny
                </Button>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
}
