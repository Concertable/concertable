import type { ArtistSummary } from "@/types/artist";
import type { Opportunity } from "@/types/opportunity";

export type ApplicationStatus =
  | "Pending"
  | "Rejected"
  | "Withdrawn"
  | "AwaitingPayment"
  | "Confirmed"
  | "Complete"
  | "Settled";

export interface OpportunityApplication {
  id: number;
  artist: ArtistSummary;
  opportunity: Opportunity;
  status: ApplicationStatus;
}
