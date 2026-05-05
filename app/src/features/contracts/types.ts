export type PaymentMethod = "Cash" | "Transfer";

interface ContractBase {
  id?: number;
  paymentMethod: PaymentMethod;
}

export interface FlatFeeContract extends ContractBase {
  $type: "flatFee";
  fee: number;
}

export interface DoorSplitContract extends ContractBase {
  $type: "doorSplit";
  artistDoorPercent: number;
}

export interface VersusContract extends ContractBase {
  $type: "versus";
  guarantee: number;
  artistDoorPercent: number;
}

export interface VenueHireContract extends ContractBase {
  $type: "venueHire";
  hireFee: number;
}

export type Contract =
  | FlatFeeContract
  | DoorSplitContract
  | VersusContract
  | VenueHireContract;
