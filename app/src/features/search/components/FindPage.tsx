import { SearchBar } from "./SearchBar";
import { SearchResults } from "./SearchResults";
import { FilterSlider } from "./FilterSlider";

export function FindPage() {
  return (
    <div className="mx-auto max-w-7xl space-y-6 p-6">
      <div className="flex items-center gap-3">
        <div className="min-w-0 flex-1">
          <SearchBar />
        </div>
        <FilterSlider />
      </div>
      <SearchResults />
    </div>
  );
}
