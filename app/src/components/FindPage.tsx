import type { HeaderType } from "@/types/header";
import { SearchBar } from "@/components/SearchBar";
import { SearchResults } from "@/components/SearchResults";
import { FilterPopover } from "@/components/FilterPopover";
import { useSearchFilters } from "@/hooks/useSearchFilters";

interface FindPageProps {
  defaultHeaderType: HeaderType;
}

export function FindPage({ defaultHeaderType }: Readonly<FindPageProps>) {
  const { filters, updateFilters } = useSearchFilters(defaultHeaderType);

  return (
    <div className="relative w-full">
      <div className="p-6 max-w-7xl mx-auto space-y-6">
        <SearchBar onSearch={updateFilters} defaultHeaderType={defaultHeaderType} />
        <SearchResults filters={filters} />
      </div>
      <div className="absolute top-6 right-6">
        <FilterPopover />
      </div>
    </div>
  );
}
