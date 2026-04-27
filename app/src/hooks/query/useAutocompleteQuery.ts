import { useQuery } from "@tanstack/react-query";
import autocompleteApi, {
  type AutocompleteResult,
} from "@/api/autocompleteApi";
import type { HeaderType } from "@/types/header";

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
