import { useSearchFilters } from "@/hooks/useSearchFilters";
import { SearchBar } from "@/components/SearchBar";
import { SearchResults } from "@/components/SearchResults";

export default function FindPage() {
  const { filters, setFilters } = useSearchFilters();

  return (
    <div className="p-6 w-full max-w-7xl mx-auto space-y-6">
      <SearchBar onSearch={setFilters} defaultHeaderType="concert" />
      <SearchResults filters={filters} />
    </div>
  );
}
