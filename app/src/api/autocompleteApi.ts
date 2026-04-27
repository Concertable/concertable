import api from "@/lib/axios";
import type { HeaderType } from "@/types/header";

export interface AutocompleteResult {
  id: number;
  name: string;
  $type: HeaderType;
}

const autocompleteApi = {
  getAutocomplete: async (
    searchTerm: string,
    headerType?: HeaderType,
  ): Promise<AutocompleteResult[]> => {
    const { data } = await api.get<AutocompleteResult[]>("/autocomplete", {
      params: { searchTerm, headerType },
    });
    return data;
  },
};

export default autocompleteApi;
