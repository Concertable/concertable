import type { Genre } from "@/types/common";
import type { Contract } from "@/features/contracts";
import type { ArtistSummary } from "@/features/artists";

export interface TicketConcert {
  id: number;
  name: string;
  price: number;
  startDate: string;
  endDate: string;
  venueName: string;
  artistName: string;
}

export interface Ticket {
  id: string;
  purchaseDate: string;
  qrCode: string;
  userId: string;
  userEmail: string;
  concert: TicketConcert;
}

export interface CheckoutSession {
  clientSecret: string;
  customerSession: string;
  customerId: string;
  intentType: "Payment" | "Setup";
}

export interface TicketCheckout {
  session: CheckoutSession;
  price: number;
  concertId: number;
}

export type PaymentTiming = "Immediate" | "Deferred" | "Authorize";

export interface FlatPayment {
  $type: "flat";
  amount: number;
}

export interface DoorSharePayment {
  $type: "doorShare";
  artistPercent: number;
}

export interface GuaranteedDoorPayment {
  $type: "guaranteedDoor";
  guarantee: number;
  artistPercent: number;
}

export type PaymentAmount =
  | FlatPayment
  | DoorSharePayment
  | GuaranteedDoorPayment;

export interface PayeeSummary {
  name: string;
  email: string | null;
}

export interface Checkout {
  timing: PaymentTiming;
  amount: PaymentAmount;
  payee: PayeeSummary;
  session: CheckoutSession;
}

export type ApplicationStatus =
  | "Pending"
  | "Rejected"
  | "Withdrawn"
  | "AwaitingPayment"
  | "Confirmed"
  | "Complete"
  | "Settled";

export interface ActionLink {
  href: string;
  method: string;
}

export interface ApplicationActions {
  accept: ActionLink;
  checkout?: ActionLink | null;
}

export interface Application {
  id: number;
  artist: ArtistSummary;
  opportunity: Opportunity;
  status: ApplicationStatus;
  actions: ApplicationActions;
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

export interface OpportunityActions {
  checkout?: ActionLink | null;
}

export interface Opportunity {
  id: number;
  venueId: number;
  startDate: string;
  endDate: string;
  genres: Genre[];
  contract: Contract;
  actions: OpportunityActions;
}

export type ContractRequest = Omit<Contract, "id">;

export interface OpportunityRequest {
  id?: number;
  startDate: string;
  endDate: string;
  genreIds: number[];
  contract: ContractRequest;
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
