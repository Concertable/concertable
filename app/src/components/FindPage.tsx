import { useState } from "react";
import type { HeaderType } from "@/types/header";
import type { SearchFilters } from "@/components/SearchBar";
import { SearchBar } from "@/components/SearchBar";
import { SearchResults } from "@/components/SearchResults";

interface FindPageProps {
  defaultHeaderType: HeaderType;
}

export function FindPage({ defaultHeaderType }: FindPageProps) {
  const [filters, setFilters] = useState<SearchFilters>({ headerType: defaultHeaderType });

  return (
    <div className="p-6 w-full max-w-7xl mx-auto space-y-6">
      <SearchBar onSearch={setFilters} defaultHeaderType={defaultHeaderType} />
      <SearchResults filters={filters} />
    </div>
  );
}
