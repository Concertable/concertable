export interface TicketPurchase {
    success: boolean;
    message: string;
    ticketId: number;
    eventId: number;
    purchaseDate: Date;
    amount: number;
    currency: string;
    transactionId: string;
    email: string
}