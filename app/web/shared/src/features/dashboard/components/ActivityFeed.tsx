import { Link } from "@tanstack/react-router";
import {
  CheckCircle2,
  CircleDollarSign,
  Inbox,
  MessageSquare,
  Star,
  Ticket,
  UserPlus,
  XCircle,
  type LucideIcon,
} from "lucide-react";
import dayjs from "dayjs";
import type { ActivityItem, ActivityType } from "@concertable/shared/features/dashboard";

const iconMap: Record<ActivityType, LucideIcon> = {
  ApplicationReceived: UserPlus,
  ApplicationAccepted: CheckCircle2,
  ApplicationDeclined: XCircle,
  ConcertSettled: CircleDollarSign,
  ReviewReceived: Star,
  TicketSold: Ticket,
  MessageReceived: MessageSquare,
};

function timestampLabel(iso: string) {
  const at = dayjs(iso);
  const hours = dayjs().diff(at, "hour");
  if (hours < 1) return "just now";
  if (hours < 24) return `${hours}h ago`;
  return at.format("D MMM");
}

export function ActivityFeed({ items }: { items: ActivityItem[] }) {
  if (items.length === 0) {
    return (
      <div className="text-muted-foreground flex flex-col items-center gap-2 py-6 text-center text-sm">
        <Inbox className="size-6 opacity-50" />
        <p>Nothing yet — activity will show here as it happens.</p>
      </div>
    );
  }

  return (
    <ul className="flex flex-col">
      {items.map((item) => {
        const Icon = iconMap[item.type] ?? Inbox;
        return (
          <li
            key={item.id}
            className="hover:bg-muted/40 -mx-2 rounded-md px-2 py-2 transition-colors"
          >
            <Link to={item.url} className="flex items-start gap-3">
              <Icon className="text-muted-foreground mt-0.5 size-4 shrink-0" />
              <div className="min-w-0 flex-1">
                <p className="text-sm leading-snug">{item.subject}</p>
                <p className="text-muted-foreground mt-0.5 text-xs">
                  {timestampLabel(item.at)}
                </p>
              </div>
            </Link>
          </li>
        );
      })}
    </ul>
  );
}
