import { useQuery } from "@tanstack/react-query";
import autocompleteApi from "../api/autocompleteApi";
import type { AutocompleteResult, HeaderType } from "../types";

export function useAutocompleteQuery(
  searchTerm: string,
  headerType?: HeaderType,
) {
  return useQuery<AutocompleteResult[]>({
    queryKey: ["autocomplete", headerType, searchTerm],
    queryFn: () => autocompleteApi.getAutocomplete(searchTerm, headerType),
    enabled: searchTerm.length > 0,
  });
}
