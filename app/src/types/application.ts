import type { ArtistSummary } from "@/features/artists";
import type { Opportunity } from "@/types/opportunity";

export type ApplicationStatus =
  | "Pending"
  | "Rejected"
  | "Withdrawn"
  | "AwaitingPayment"
  | "Confirmed"
  | "Complete"
  | "Settled";

export interface Application {
  id: number;
  artist: ArtistSummary;
  opportunity: Opportunity;
  status: ApplicationStatus;
}

interface PaymentResponse {
  requiresAction: boolean;
  clientSecret?: string;
  transactionId?: string;
}

interface ImmediateAcceptOutcome {
  $type: "immediate";
  payment: PaymentResponse;
}

interface DeferredAcceptOutcome {
  $type: "deferred";
}

export type AcceptOutcome = ImmediateAcceptOutcome | DeferredAcceptOutcome;
