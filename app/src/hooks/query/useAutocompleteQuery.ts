import { useQuery } from "@tanstack/react-query";
import {
  getHeaderAutocomplete,
  type AutocompleteResult,
} from "@/api/autocompleteApi";

export function useHeaderAutocompleteQuery(searchTerm: string) {
  return useQuery<AutocompleteResult[]>({
    queryKey: ["autocomplete", "headers", searchTerm],
    queryFn: () => getHeaderAutocomplete(searchTerm),
    enabled: searchTerm.length > 0,
  });
}
