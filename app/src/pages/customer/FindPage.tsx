import { SearchBar } from "@/components/SearchBar";
import type { SearchFilters } from "@/components/SearchBar";

export default function FindPage() {
  function handleSearch(filters: SearchFilters) {
    console.log(filters);
  }

  return (
    <div className="p-6 w-full max-w-7xl mx-auto">
      <SearchBar onSearch={handleSearch} />
    </div>
  );
}
