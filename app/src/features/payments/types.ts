export type PayoutAccountStatus = "NotVerified" | "Pending" | "Verified";

export interface PaymentMethod {
  brand: string;
  last4: string;
  expMonth: number;
  expYear: number;
}
