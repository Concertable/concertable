import type { Genre } from "@/types/common";
import type { Contract } from "@/features/contracts";

export interface Opportunity {
  id: number;
  venueId: number;
  startDate: string;
  endDate: string;
  genres: Genre[];
  contract: Contract;
}
