import type { Genre } from "@/types/common";
import type { Contract } from "@/types/contract";

export interface Opportunity {
  id: number;
  venueId: number;
  startDate: string;
  endDate: string;
  genres: Genre[];
  contract: Contract;
}
