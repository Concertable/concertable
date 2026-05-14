import type { Genre } from "@/types/common";
import type { User } from "@/features/auth";

export interface Preference {
  id: number;
  user: User;
  radiusKm: number;
  genres: Genre[];
}

export interface CreatePreferenceRequest {
  radiusKm: number;
  genres: Genre[];
}
