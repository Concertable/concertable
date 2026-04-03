import type { Concert } from "@/types/concert";
import { Button } from "@/components/ui/button";
import { useNavigate } from "@tanstack/react-router";
import dayjs from "dayjs";
import { CalendarDays, MapPin, Ticket } from "lucide-react";

interface Props {
  concert: Concert;
}

export function ConcertCard({ concert }: Readonly<Props>) {
  const navigate = useNavigate();

  return (
    <div className="rounded-xl border border-border bg-card p-4 space-y-4">
      <img
        src={concert.artist.imageUrl}
        alt={concert.artist.name}
        className="w-full rounded-lg object-cover aspect-square"
      />

      <div className="space-y-2 text-sm">
        <div className="flex items-center gap-2 text-muted-foreground">
          <CalendarDays className="size-4 shrink-0" />
          <span>{dayjs(concert.startDate).format("D MMM YYYY, HH:mm")} – {dayjs(concert.endDate).format("HH:mm")}</span>
        </div>

        <div className="flex items-center gap-2 text-muted-foreground">
          <MapPin className="size-4 shrink-0" />
          <span>{concert.venue.name}, {concert.venue.town}</span>
        </div>

        <div className="flex items-center gap-2 text-muted-foreground">
          <Ticket className="size-4 shrink-0" />
          <span>£{concert.price.toFixed(2)} · {concert.availableTickets} left</span>
        </div>
      </div>

      <Button className="w-full" onClick={() => void navigate({ to: "/concert/checkout/$id", params: { id: concert.id } })}>
        Buy Tickets
      </Button>
    </div>
  );
}
