import type {
  Contract,
  FlatFeeContract,
  DoorSplitContract,
  VersusContract,
  VenueHireContract,
} from "../types";

const contractTypeLabel: Record<Contract["$type"], string> = {
  flatFee: "Flat Fee",
  doorSplit: "Door Split",
  versus: "Versus",
  venueHire: "Venue Hire",
};

const contractSummaryRegistry = {
  flatFee: (c: FlatFeeContract) => `£${c.fee}`,
  doorSplit: (c: DoorSplitContract) => `${c.artistDoorPercent}% door`,
  versus: (c: VersusContract) => `£${c.guarantee} vs ${c.artistDoorPercent}%`,
  venueHire: (c: VenueHireContract) => `£${c.hireFee} hire fee`,
} as Record<Contract["$type"], (contract: Contract) => string>;

interface Props {
  contract: Contract;
}

export function ContractSummaryLabel({ contract }: Readonly<Props>) {
  return (
    <p className="font-medium">
      {contractTypeLabel[contract.$type]}{" "}
      <span className="text-muted-foreground text-sm font-normal">
        · {contractSummaryRegistry[contract.$type](contract)}
      </span>
    </p>
  );
}
