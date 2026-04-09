import { useQuery } from "@tanstack/react-query";
import autocompleteApi, {
  type AutocompleteResult,
} from "@/api/autocompleteApi";

export function useHeaderAutocompleteQuery(searchTerm: string) {
  return useQuery<AutocompleteResult[]>({
    queryKey: ["autocomplete", "headers", searchTerm],
    queryFn: () => autocompleteApi.getHeaderAutocomplete(searchTerm),
    enabled: searchTerm.length > 0,
  });
}
