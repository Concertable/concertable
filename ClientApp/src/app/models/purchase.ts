export interface Purchase {
    success: boolean;
    requiresAction: boolean;
    message: string;
    purchaseDate: Date;
    amount: number;
    currency: string;
    transactionId: string;
    clientSecret: string;
    email: string
}