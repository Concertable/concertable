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
    <div className="border-border bg-card space-y-4 rounded-xl border p-4">
      <img
        src={concert.artist.imageUrl}
        alt={concert.artist.name}
        className="aspect-square w-full rounded-lg object-cover"
      />

      <div className="space-y-2 text-sm">
        <div className="text-muted-foreground flex items-center gap-2">
          <CalendarDays className="size-4 shrink-0" />
          <span>
            {dayjs(concert.startDate).format("D MMM YYYY, HH:mm")} –{" "}
            {dayjs(concert.endDate).format("HH:mm")}
          </span>
        </div>

        <div className="text-muted-foreground flex items-center gap-2">
          <MapPin className="size-4 shrink-0" />
          <span>
            {concert.venue.name}, {concert.venue.town}
          </span>
        </div>

        <div className="text-muted-foreground flex items-center gap-2">
          <Ticket className="size-4 shrink-0" />
          <span>
            £{concert.price.toFixed(2)} · {concert.availableTickets} left
          </span>
        </div>
      </div>

      <Button
        className="w-full"
        onClick={() =>
          void navigate({
            to: "/concert/checkout/$id",
            params: { id: concert.id },
          })
        }
      >
        Buy Tickets
      </Button>
    </div>
  );
}
