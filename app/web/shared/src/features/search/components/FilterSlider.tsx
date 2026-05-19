import { useState } from "react";
import { FilterIcon, PlusIcon, XIcon } from "lucide-react";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Slider } from "@/components/ui/slider";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { useSearchFiltersStore } from "../store/useSearchFiltersStore";
import { useSearchFilters } from "../hooks/useSearchFilters";
import { useGenresQuery } from "../hooks/useGenreQuery";
import type { SearchFilters } from "../schemas/searchSchema";
import { genreLabel } from "@/types/common";
import type { Genre } from "@/types/common";

const ORDER_BY_OPTIONS = [
  { value: "name", label: "Name" },
  { value: "rating", label: "Rating" },
  { value: "distance", label: "Distance" },
];

export function FilterSlider() {
  const { filters, setFilters } = useSearchFiltersStore();
  const { updateFilters } = useSearchFilters();
  const { data: genres } = useGenresQuery();
  const [open, setOpen] = useState(false);
  const [pendingGenre, setPendingGenre] = useState<Genre | "">("");

  function update(next: Partial<SearchFilters>) {
    setFilters({ ...filters, ...next });
  }

  function addGenre() {
    if (!pendingGenre) return;
    if (filters.genres?.includes(pendingGenre)) return;
    update({ genres: [...(filters.genres ?? []), pendingGenre] });
    setPendingGenre("");
  }

  function apply() {
    updateFilters(filters);
    setOpen(false);
  }

  const selectedGenres = genres?.filter((g) => filters.genres?.includes(g)) ?? [];
  const availableGenres = genres?.filter((g) => !filters.genres?.includes(g)) ?? [];

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger asChild>
        <Button
          variant="secondary"
          size="icon"
          className="shrink-0 rounded-full"
          data-testid="filter-open"
        >
          <FilterIcon />
        </Button>
      </SheetTrigger>

      <SheetContent data-testid="filter-panel">
        <SheetHeader>
          <SheetTitle>Filter</SheetTitle>
        </SheetHeader>

        <div className="space-y-6 px-4 pt-2">
          <div className="space-y-1.5">
            <p className="text-muted-foreground text-xs">Header Type</p>
            <Select
              value={filters.headerType}
              onValueChange={(v) =>
                update({ headerType: v as SearchFilters["headerType"] })
              }
            >
              <SelectTrigger className="w-full" data-testid="filter-header-type">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="concert">Concert</SelectItem>
                <SelectItem value="artist">Artist</SelectItem>
                <SelectItem value="venue">Venue</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <p className="text-muted-foreground text-xs">Genre</p>
            <div className="flex gap-2">
              <Select value={pendingGenre} onValueChange={(v) => setPendingGenre(v as Genre)}>
                <SelectTrigger className="flex-1" data-testid="filter-genre-select">
                  <SelectValue placeholder="Select genre" />
                </SelectTrigger>
                <SelectContent>
                  {availableGenres.map((g) => (
                    <SelectItem key={g} value={g}>
                      {genreLabel(g)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <Button size="icon" onClick={addGenre} disabled={!pendingGenre} data-testid="filter-genre-add">
                <PlusIcon />
              </Button>
            </div>
            {selectedGenres.length > 0 && (
              <div className="flex flex-wrap gap-1.5">
                {selectedGenres.map((g) => (
                  <span
                    key={g}
                    className="bg-muted flex items-center gap-1 rounded-full px-2.5 py-0.5 text-xs"
                  >
                    {genreLabel(g)}
                    <button
                      onClick={() =>
                        update({ genres: filters.genres?.filter((x) => x !== g) })
                      }
                      className="text-muted-foreground hover:text-foreground"
                    >
                      <XIcon size={12} />
                    </button>
                  </span>
                ))}
              </div>
            )}
          </div>

          <div className="space-y-2">
            <div className="flex justify-between">
              <p className="text-muted-foreground text-xs">
                Distance Radius (km)
              </p>
              <span className="text-xs font-medium" data-testid="filter-radius-value">
                {filters.radius ?? 50} km
              </span>
            </div>
            <Slider
              min={1}
              max={200}
              step={1}
              defaultValue={[filters.radius ?? 50]}
              onValueChange={([v]) => update({ radius: v })}
              data-testid="filter-radius-slider"
            />
          </div>

          <div className="flex gap-2">
            <Select
              value={filters.orderBy ?? ""}
              onValueChange={(v) => update({ orderBy: v })}
            >
              <SelectTrigger className="flex-1" data-testid="filter-order-by">
                <SelectValue placeholder="Order By" />
              </SelectTrigger>
              <SelectContent>
                {ORDER_BY_OPTIONS.map((o) => (
                  <SelectItem key={o.value} value={o.value}>
                    {o.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select
              value={filters.sortOrder ?? ""}
              onValueChange={(v) => update({ sortOrder: v as "asc" | "desc" })}
            >
              <SelectTrigger className="flex-1" data-testid="filter-sort-order">
                <SelectValue placeholder="Sort Order" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="asc">Ascending</SelectItem>
                <SelectItem value="desc">Descending</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="flex items-center gap-4">
            <label className="flex cursor-pointer items-center gap-2 text-sm">
              <Checkbox
                checked={filters.showHistory ?? false}
                onCheckedChange={(v) => update({ showHistory: !!v })}
                data-testid="filter-show-history"
              />
              Show History
            </label>
            <label className="flex cursor-pointer items-center gap-2 text-sm">
              <Checkbox
                checked={filters.showSold ?? false}
                onCheckedChange={(v) => update({ showSold: !!v })}
                data-testid="filter-show-sold"
              />
              Show Sold
            </label>
          </div>

          <Button className="w-full" onClick={apply} data-testid="filter-apply">
            Apply
          </Button>
        </div>
      </SheetContent>
    </Sheet>
  );
}
