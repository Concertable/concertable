import { SearchBar } from "@/components/SearchBar";
import { SearchResults } from "@/components/SearchResults";
import { FilterSlider } from "@/components/FilterSlider";

export function FindPage() {
  return (
    <div className="relative w-full">
      <div className="p-6 max-w-7xl mx-auto space-y-6">
        <SearchBar />
        <SearchResults />
      </div>
      <div className="absolute top-6 right-6">
        <FilterSlider />
      </div>
    </div>
  );
}
