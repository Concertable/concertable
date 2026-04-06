import { useState, useRef } from "react";
import { useNavigate } from "@tanstack/react-router";
import { SearchIcon } from "lucide-react";
import { Input } from "@/components/ui/input";
import { useHeaderAutocompleteQuery } from "@/hooks/query/useAutocompleteQuery";
import type { AutocompleteResult } from "@/api/autocompleteApi";
import type { HeaderType } from "@/types/header";

export function NavbarSearch() {
  const [search, setSearch] = useState("");
  const navigate = useNavigate();
  const { data, isLoading } = useHeaderAutocompleteQuery(search);
  const ref = useRef<HTMLDivElement>(null);

  function handleSelect(result: AutocompleteResult) {
    setSearch("");
    navigate({
      to: `/find/${result.$type}/$id` as `/find/${HeaderType}/$id`,
      params: { id: result.id },
    });
  }

  return (
    <div ref={ref} className="relative">
      <SearchIcon className="text-primary-foreground/50 absolute top-1/2 left-3 z-10 size-4 -translate-y-1/2" />
      <Input
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        onKeyDown={(e) => {
          if (e.key === "Enter" && search) {
            setSearch("");
            navigate({
              to: "/find",
              search: () => ({ headerType: "concert" as const, query: search }),
            });
          }
        }}
        placeholder="Search..."
        className="text-primary-foreground placeholder:text-primary-foreground/50 w-56 border-white/20 bg-white/10 pl-9"
      />
      {(isLoading || !!data?.length) && (
        <div className="bg-popover text-popover-foreground absolute top-full right-0 z-50 mt-1 w-64 rounded-lg border shadow-md">
          {isLoading && (
            <p className="text-muted-foreground px-3 py-2 text-sm">
              Loading...
            </p>
          )}
          {data?.map((result, i) => (
            <button
              key={`${result.$type}-${i}`}
              onMouseDown={() => handleSelect(result)}
              className="hover:bg-accent flex w-full items-center justify-between px-3 py-2 text-left text-sm first:rounded-t-lg last:rounded-b-lg"
            >
              <span className="truncate font-medium">{result.name}</span>
              <span className="text-muted-foreground ml-2 shrink-0 text-xs capitalize">
                {result.$type}
              </span>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
