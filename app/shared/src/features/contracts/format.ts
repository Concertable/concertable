import type {
  Contract,
  DoorSplitContract,
  FlatFeeContract,
  VenueHireContract,
  VersusContract,
} from "./types";

const summaryRegistry: Record<Contract["$type"], (contract: Contract) => string> = {
  flatFee: (c) => `£${(c as FlatFeeContract).fee}`,
  doorSplit: (c) => `${(c as DoorSplitContract).artistDoorPercent}% door`,
  versus: (c) => {
    const v = c as VersusContract;
    return `£${v.guarantee} vs ${v.artistDoorPercent}%`;
  },
  venueHire: (c) => `£${(c as VenueHireContract).hireFee} hire fee`,
};

export function contractSummary(contract: Contract): string {
  return summaryRegistry[contract.$type](contract);
}
