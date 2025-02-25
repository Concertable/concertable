import { Event } from "./event";
import { Purchase } from "./purchase";

export interface ListingApplicationPurchase extends Purchase {
    applicationId: number;
    event: Event
}