import type { Artist } from "@/types/artist";
import type { Opportunity } from "@/types/opportunity";

export type ApplicationStatus =
  | "Pending"
  | "Rejected"
  | "Withdrawn"
  | "AwaitingPayment"
  | "Confirmed"
  | "Complete"
  | "Settled";

export interface ConcertApplication {
  id: number;
  artist: Artist;
  opportunity: Opportunity;
  status: ApplicationStatus;
}
