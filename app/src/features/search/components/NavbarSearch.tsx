import { useState } from "react";
import { useNavigate } from "@tanstack/react-router";
import { SearchIcon } from "lucide-react";
import { Input } from "@/components/ui/input";
import { AutocompleteDropdown } from "./AutocompleteDropdown";
import { useSearchState } from "../hooks/useSearchState";
import type { AutocompleteResult, HeaderType } from "../types";

export function NavbarSearch() {
  const [search, setSearch] = useState("");
  const navigate = useNavigate();
  const { open, close, inputProps } = useSearchState();

  function handleSelect(result: AutocompleteResult) {
    setSearch("");
    close();
    navigate({
      to: `/find/${result.$type}/$id` as `/find/${HeaderType}/$id`,
      params: { id: result.id },
    });
  }

  return (
    <div className="relative">
      <SearchIcon className="text-primary-foreground/50 absolute top-1/2 left-3 z-10 size-4 -translate-y-1/2" />
      <Input
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        onKeyDown={(e) => {
          if (e.key === "Enter" && search) {
            setSearch("");
            close();
            navigate({
              to: "/find",
              search: () => ({ headerType: "concert" as const, query: search }),
            });
          }
        }}
        {...inputProps}
        placeholder="Search..."
        className="text-primary-foreground placeholder:text-primary-foreground/50 w-56 border-white/20 bg-white/10 pl-9"
      />
      {open && (
        <AutocompleteDropdown
          searchTerm={search}
          onSelect={handleSelect}
          className="right-0 left-0"
        />
      )}
    </div>
  );
}
