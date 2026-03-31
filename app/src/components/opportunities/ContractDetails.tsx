import type { ComponentType } from "react";
import type { Contract, FlatFeeContract, DoorSplitContract, VersusContract, VenueHireContract } from "@/types/contract";

function FlatFeeDetails({ contract }: { contract: FlatFeeContract }) {
  return (
    <div className="space-y-1">
      <p className="text-sm text-muted-foreground">Type</p>
      <p className="font-medium">Flat Fee</p>
      <p className="text-sm text-muted-foreground mt-2">Fee</p>
      <p className="font-medium">£{contract.fee}</p>
      <p className="text-sm text-muted-foreground mt-2">Payment</p>
      <p className="font-medium">{contract.paymentMethod}</p>
    </div>
  );
}

function DoorSplitDetails({ contract }: { contract: DoorSplitContract }) {
  return (
    <div className="space-y-1">
      <p className="text-sm text-muted-foreground">Type</p>
      <p className="font-medium">Door Split</p>
      <p className="text-sm text-muted-foreground mt-2">Artist Door %</p>
      <p className="font-medium">{contract.artistDoorPercent}%</p>
      <p className="text-sm text-muted-foreground mt-2">Payment</p>
      <p className="font-medium">{contract.paymentMethod}</p>
    </div>
  );
}

function VersusDetails({ contract }: { contract: VersusContract }) {
  return (
    <div className="space-y-1">
      <p className="text-sm text-muted-foreground">Type</p>
      <p className="font-medium">Versus</p>
      <p className="text-sm text-muted-foreground mt-2">Guarantee</p>
      <p className="font-medium">£{contract.guarantee}</p>
      <p className="text-sm text-muted-foreground mt-2">Artist Door %</p>
      <p className="font-medium">{contract.artistDoorPercent}%</p>
      <p className="text-sm text-muted-foreground mt-2">Payment</p>
      <p className="font-medium">{contract.paymentMethod}</p>
    </div>
  );
}

function VenueHireDetails({ contract }: { contract: VenueHireContract }) {
  return (
    <div className="space-y-1">
      <p className="text-sm text-muted-foreground">Type</p>
      <p className="font-medium">Venue Hire</p>
      <p className="text-sm text-muted-foreground mt-2">Hire Fee</p>
      <p className="font-medium">£{contract.hireFee}</p>
      <p className="text-sm text-muted-foreground mt-2">Payment</p>
      <p className="font-medium">{contract.paymentMethod}</p>
    </div>
  );
}

const contractRegistry = {
  flatFee: FlatFeeDetails,
  doorSplit: DoorSplitDetails,
  versus: VersusDetails,
  venueHire: VenueHireDetails,
} as Record<Contract["$type"], ComponentType<{ contract: Contract }>>;

interface Props {
  contract: Contract;
}

export function ContractDetails({ contract }: Readonly<Props>) {
  const Component = contractRegistry[contract.$type];
  return <Component contract={contract} />;
}
