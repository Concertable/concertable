import { searchClient } from "../../../lib/searchClient";
import type { AutocompleteResult, HeaderType } from "../types";

const autocompleteApi = {
  getAutocomplete: async (
    searchTerm: string,
    headerType?: HeaderType,
  ): Promise<AutocompleteResult[]> => {
    const { data } = await searchClient.get<AutocompleteResult[]>("/autocomplete", {
      params: { searchTerm, headerType },
    });
    return data;
  },
};

export default autocompleteApi;
