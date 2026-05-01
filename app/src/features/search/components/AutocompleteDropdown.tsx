import { cn } from "@/lib/utils";
import { useAutocompleteQuery } from "../hooks/useAutocompleteQuery";
import type { AutocompleteResult, HeaderType } from "../types";

interface AutocompleteDropdownProps {
  searchTerm: string;
  onSelect: (result: AutocompleteResult) => void;
  headerType?: HeaderType;
  className?: string;
}

export function AutocompleteDropdown({
  searchTerm,
  onSelect,
  headerType,
  className,
}: AutocompleteDropdownProps) {
  const { data, isLoading } = useAutocompleteQuery(searchTerm, headerType);

  if (!isLoading && !data?.length) return null;

  return (
    <div
      className={cn(
        "bg-popover text-popover-foreground absolute top-full z-50 mt-1 rounded-lg border shadow-md",
        className,
      )}
    >
      {isLoading && (
        <p className="text-muted-foreground px-3 py-2 text-sm">Loading...</p>
      )}
      {data?.map((result, i) => (
        <button
          key={`${result.$type}-${i}`}
          onMouseDown={() => onSelect(result)}
          className="hover:bg-accent flex w-full items-center justify-between px-3 py-2 text-left text-sm first:rounded-t-lg last:rounded-b-lg"
        >
          <span className="truncate font-medium">{result.name}</span>
          <span className="text-muted-foreground ml-2 shrink-0 text-xs capitalize">
            {result.$type}
          </span>
        </button>
      ))}
    </div>
  );
}
