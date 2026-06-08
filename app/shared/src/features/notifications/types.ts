import type { Message } from "../messaging/types";

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
export type ConcertDraftCreatedPayload = number;
export type ApplicationAcceptedPayload = number;
