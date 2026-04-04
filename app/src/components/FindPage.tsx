import { SearchBar } from "@/components/SearchBar";
import { SearchResults } from "@/components/SearchResults";
import { FilterSlider } from "@/components/FilterSlider";

export function FindPage() {
  return (
    <div className="relative w-full">
      <div className="mx-auto max-w-7xl space-y-6 p-6">
        <SearchBar />
        <SearchResults />
      </div>
      <div className="absolute top-6 right-6">
        <FilterSlider />
      </div>
    </div>
  );
}
