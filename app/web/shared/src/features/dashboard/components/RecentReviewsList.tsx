import { Link } from "@tanstack/react-router";
import dayjs from "dayjs";
import { Star } from "lucide-react";
import type {
  ReviewExcerpt,
  ReviewSummary,
} from "@concertable/shared/features/dashboard";

function timestampLabel(iso: string) {
  const at = dayjs(iso);
  const hours = dayjs().diff(at, "hour");
  if (hours < 1) return "just now";
  if (hours < 24) return `${hours}h ago`;
  const days = dayjs().diff(at, "day");
  if (days < 30) return `${days}d ago`;
  return at.format("D MMM");
}

function StarRow({ stars }: { stars: number }) {
  return (
    <div className="flex items-center gap-0.5" aria-label={`${stars} out of 5 stars`}>
      {Array.from({ length: 5 }).map((_, i) => (
        <Star
          key={i}
          className={
            i < stars
              ? "size-3.5 fill-amber-400 text-amber-400"
              : "text-muted-foreground/30 size-3.5"
          }
        />
      ))}
    </div>
  );
}

export function RecentReviewsList({
  summary,
  items,
}: {
  summary?: ReviewSummary;
  items: ReviewExcerpt[];
}) {
  const total = summary?.totalReviews ?? 0;

  if (total === 0 && items.length === 0) {
    return (
      <div className="text-muted-foreground flex flex-col items-center gap-2 py-6 text-center text-sm">
        <Star className="size-6 opacity-50" />
        <p>No reviews yet — they land here after concerts settle.</p>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-3">
      {summary && total > 0 && (
        <div className="border-border flex items-baseline justify-between border-b pb-2">
          <div className="flex items-baseline gap-1">
            <span className="text-2xl font-semibold tabular-nums">
              {summary.averageRating?.toFixed(1)}
            </span>
            <Star className="size-4 fill-amber-400 text-amber-400" />
          </div>
          <span className="text-muted-foreground text-xs">
            {total} {total === 1 ? "review" : "reviews"}
          </span>
        </div>
      )}

      <ul className="flex flex-col">
        {items.map((item) => (
          <li
            key={item.id}
            className="hover:bg-muted/40 -mx-2 rounded-md px-2 py-2 transition-colors"
          >
            <Link to={item.href} className="flex flex-col gap-1">
              <div className="flex items-center justify-between gap-2">
                <span className="truncate text-sm font-medium">
                  {item.reviewerName}
                </span>
                <StarRow stars={item.stars} />
              </div>
              {item.excerpt && (
                <p className="text-muted-foreground line-clamp-2 text-xs leading-snug">
                  {item.excerpt}
                </p>
              )}
              <p className="text-muted-foreground text-[11px]">
                {timestampLabel(item.at)}
              </p>
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
}
