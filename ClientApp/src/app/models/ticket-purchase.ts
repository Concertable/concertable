import { Purchase } from "./purchase";

export interface TicketPurchase extends Purchase {
    ticketId: number;
    eventId: number;
}