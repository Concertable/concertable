import type { ActionLink, Genre } from "../../types/common";

export interface CheckoutSession {
  clientSecret: string;
  customerSession?: string;
  customerId?: string;
}

export interface CheckoutLabels {
  summaryTitle: string;
  submitLabel: string;
  paymentHint: string | null;
}

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
  amount: PaymentAmount;
  payee: PayeeSummary;
  session: CheckoutSession;
  labels: CheckoutLabels;
}

export type { ESignatureRequest } from "./schemas/eSignatureRequestSchema";

export interface PaymentResponse {
  requiresAction: boolean;
  clientSecret?: string;
  transactionId?: string;
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

export interface ConcertActions {
  cancel?: ActionLink | null;
  contract?: ActionLink | null;
  declareDoorRevenue?: ActionLink | null;
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
  // Venue-private; present only on the owner (party-scoped) read, alongside actions.
  ticketsSold?: number | null;
  doorRevenue?: number | null;
  actions?: ConcertActions;
}
