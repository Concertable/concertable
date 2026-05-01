import api from "@/lib/axios";
import type { AutocompleteResult, HeaderType } from "../types";

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
