import type { Genre } from "@/types/common";
import type { Contract } from "@/features/contracts";
import type { ArtistSummary } from "@/features/artists";

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

export interface Opportunity {
  id: number;
  venueId: number;
  startDate: string;
  endDate: string;
  genres: Genre[];
  contract: Contract;
}

export interface ConcertArtist {
  id: number;
  name: string;
  avatar?: string;
  rating: number;
  county: string;
  town: string;
  genres: Genre[];
}

export interface ConcertVenue {
  id: number;
  name: string;
  county: string;
  town: string;
  latitude: number;
  longitude: number;
}

export interface Concert {
  id: number;
  name: string;
  about: string;
  bannerUrl: string;
  avatar: string;
  rating: number;
  price: number;
  totalTickets: number;
  availableTickets: number;
  startDate: string;
  endDate: string;
  datePosted?: string;
  venue: ConcertVenue;
  artist: ConcertArtist;
  genres: Genre[];
}
