export type PayoutAccountStatus = "notVerified" | "pending" | "verified";

export interface PaymentMethod {
  brand: string;
  last4: string;
  expMonth: number;
  expYear: number;
}

export interface PaymentOutcome {
  requiresAction: boolean;
  clientSecret?: string;
  transactionId?: string;
}
