import type { Message } from "@/types/message";
import type { Concert } from "@/types/concert";

export interface TicketPurchasedPayload {
  success: boolean;
  requiresAction: boolean;
  message: string;
  amount: number;
  currency?: string;
  purchaseDate: string;
  transactionId?: string;
  clientSecret?: string;
  userEmail?: string;
  ticketIds: number[];
  concertId: number;
}

export interface ConcertPostedPayload {
  id: number;
  name: string;
  imageUrl: string;
  rating?: number;
  county: string;
  town: string;
  latitude: number;
  longitude: number;
  startDate: string;
  endDate: string;
  datePosted?: string;
}

export type MessageReceivedPayload = Message;
export type ConcertDraftCreatedPayload = Concert;
