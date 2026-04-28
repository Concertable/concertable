import type { CheckoutSession } from "@/types/checkoutSession";

export type PaymentTiming = "immediate" | "deferred";

export interface FlatPayment {
  $type: "flat";
  amount: number;
}

export interface DoorSharePayment {
  $type: "doorShare";
  artistPercent: number;
}

export interface GuaranteedDoorPayment {
  $type: "guaranteedDoor";
  guarantee: number;
  artistPercent: number;
}

export type PaymentAmount =
  | FlatPayment
  | DoorSharePayment
  | GuaranteedDoorPayment;

export interface PayeeSummary {
  name: string;
  email: string | null;
}

export interface AcceptCheckout {
  timing: PaymentTiming;
  amount: PaymentAmount;
  payee: PayeeSummary;
  session: CheckoutSession;
}
