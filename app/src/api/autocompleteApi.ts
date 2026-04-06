import api from "@/lib/axios";
import type { HeaderType } from "@/types/header";

export interface AutocompleteResult {
  id: number;
  name: string;
  $type: HeaderType;
}

export async function getHeaderAutocomplete(
  searchTerm: string,
): Promise<AutocompleteResult[]> {
  const { data } = await api.get<AutocompleteResult[]>(
    "/autocomplete/headers",
    {
      params: { searchTerm },
    },
  );
  return data;
}
