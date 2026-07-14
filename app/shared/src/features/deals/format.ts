import type {
  Deal,
  DoorSplitDeal,
  FlatFeeDeal,
  VenueHireDeal,
  VersusDeal,
} from "./types";

const summaryRegistry: Record<Deal["$type"], (contract: Deal) => string> = {
  flatFee: (c) => `£${(c as FlatFeeDeal).fee}`,
  doorSplit: (c) => `${(c as DoorSplitDeal).artistDoorPercent}% door`,
  versus: (c) => {
    const v = c as VersusDeal;
    return `£${v.guarantee} vs ${v.artistDoorPercent}%`;
  },
  venueHire: (c) => `£${(c as VenueHireDeal).hireFee} hire fee`,
};

export function dealSummary(contract: Deal): string {
  return summaryRegistry[contract.$type](contract);
}
