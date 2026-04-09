import api from "@/lib/axios";
import type { HeaderType } from "@/types/header";

export interface AutocompleteResult {
  id: number;
  name: string;
  $type: HeaderType;
}

const autocompleteApi = {
  getHeaderAutocomplete: async (
    searchTerm: string,
  ): Promise<AutocompleteResult[]> => {
    const { data } = await api.get<AutocompleteResult[]>(
      "/autocomplete/headers",
      {
        params: { searchTerm },
      },
    );
    return data;
  },
};

export default autocompleteApi;
