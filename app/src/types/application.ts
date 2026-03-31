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
  opportunity: Opportunity;
  status: ApplicationStatus;
}
