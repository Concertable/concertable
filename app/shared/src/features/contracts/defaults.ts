import type { Contract, PaymentMethod } from "./types";

export const CONTRACT_TYPE_LABELS: Record<Contract["$type"], string> = {
  flatFee: "Flat Fee",
  doorSplit: "Door Split",
  versus: "Versus",
  venueHire: "Venue Hire",
};

export function defaultContract(
  type: Contract["$type"],
  paymentMethod: PaymentMethod = "Transfer",
): Contract {
  switch (type) {
    case "flatFee":
      return { $type: "flatFee", paymentMethod, fee: 0 };
    case "doorSplit":
      return { $type: "doorSplit", paymentMethod, artistDoorPercent: 70 };
    case "versus":
      return {
        $type: "versus",
        paymentMethod,
        guarantee: 0,
        artistDoorPercent: 70,
      };
    case "venueHire":
      return { $type: "venueHire", paymentMethod, hireFee: 0 };
  }
}
