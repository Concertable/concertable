import { SearchBar } from "@/components/SearchBar";
import { SearchResults } from "@/components/SearchResults";
import { FilterSlider } from "@/components/FilterSlider";
import { useSearchFilters } from "@/hooks/useSearchFilters";

export function FindPage() {
  const { updateFilters } = useSearchFilters();

  return (
    <div className="relative w-full">
      <div className="p-6 max-w-7xl mx-auto space-y-6">
        <SearchBar onSearch={updateFilters} />
        <SearchResults />
      </div>
      <div className="absolute top-6 right-6">
        <FilterSlider onApply={updateFilters} />
      </div>
    </div>
  );
}
