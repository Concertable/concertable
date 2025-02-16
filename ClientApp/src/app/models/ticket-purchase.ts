export interface TicketPurchase {
    success: boolean;
    requiresAction: boolean;
    message: string;
    ticketId: number;
    eventId: number;
    purchaseDate: Date;
    amount: number;
    currency: string;
    transactionId: string;
    clientSecret: string;
    email: string
}