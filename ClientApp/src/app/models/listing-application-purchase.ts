import { Purchase } from "./purchase";

export interface ListingApplicationPurchase extends Purchase {
    applicationId: number;
}