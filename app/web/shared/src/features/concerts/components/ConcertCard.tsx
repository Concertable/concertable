import { Button } from "@/components/ui/button";
import { useImageUrl } from "@concertable/shared/hooks";
import dayjs from "dayjs";
import { CalendarDays, MapPin, Ticket } from "lucide-react";
import type { Concert } from "../types";

interface Props {
  concert: Concert;
  onBuyTickets?: () => void;
}

export function ConcertCard({ concert, onBuyTickets }: Readonly<Props>) {
  const { data: src } = useImageUrl(concert.avatar);

  return (
    <div className="border-border bg-card space-y-4 rounded-xl border p-4">
      <img
        src={src}
        alt={concert.artist.name}
        className="aspect-square w-full rounded-lg object-cover"
      />

      <div className="space-y-2 text-sm">
        <div className="text-muted-foreground flex items-center gap-2">
          <CalendarDays className="size-4 shrink-0" />
          <span>
            {dayjs(concert.startDate).format("D MMM YYYY, HH:mm")} â€“{" "}
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
            Â£{concert.price.toFixed(2)} Â· {concert.availableTickets} left
          </span>
        </div>
      </div>

      <Button
        className="w-full"
        data-testid="buy-tickets"
        disabled={!onBuyTickets}
        onClick={onBuyTickets}
      >
        Buy Tickets
      </Button>
    </div>
  );
}
