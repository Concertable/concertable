import { useState } from "react";
import { FunnelIcon, PlusIcon, XIcon } from "lucide-react";
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
import { useSearchFiltersStore } from "@/store/useSearchFiltersStore";
import { useSearchFilters } from "@/hooks/useSearchFilters";
import { useGenresQuery } from "@/hooks/query/useGenreQuery";
import type { SearchFilters } from "@/schemas/searchSchema";

const ORDER_BY_OPTIONS = [
  { value: "name", label: "Name" },
  { value: "rating", label: "Rating" },
  { value: "distance", label: "Distance" },
];

export function FilterSlider() {
  const { filters, setFilters } = useSearchFiltersStore();
  const { updateFilters } = useSearchFilters();
  const { data: genres } = useGenresQuery();
  const [pendingGenre, setPendingGenre] = useState("");

  function update(next: Partial<SearchFilters>) {
    setFilters({ ...filters, ...next });
  }

  function addGenre() {
    if (!pendingGenre) return;
    const id = Number(pendingGenre);
    if (filters.genreIds?.includes(id)) return;
    update({ genreIds: [...(filters.genreIds ?? []), id] });
    setPendingGenre("");
  }

  function apply() {
    updateFilters(filters);
  }

  const selectedGenres =
    genres?.filter((g) => filters.genreIds?.includes(g.id)) ?? [];
  const availableGenres =
    genres?.filter((g) => !filters.genreIds?.includes(g.id)) ?? [];

  return (
    <Sheet>
      <SheetTrigger asChild>
        <Button variant="outline" size="icon" className="shrink-0 rounded-full">
          <FunnelIcon />
        </Button>
      </SheetTrigger>

      <SheetContent>
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
              <SelectTrigger className="w-full">
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
              <Select value={pendingGenre} onValueChange={setPendingGenre}>
                <SelectTrigger className="flex-1">
                  <SelectValue placeholder="Select genre" />
                </SelectTrigger>
                <SelectContent>
                  {availableGenres.map((g) => (
                    <SelectItem key={g.id} value={String(g.id)}>
                      {g.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <Button size="icon" onClick={addGenre} disabled={!pendingGenre}>
                <PlusIcon />
              </Button>
            </div>
            {selectedGenres.length > 0 && (
              <div className="flex flex-wrap gap-1.5">
                {selectedGenres.map((g) => (
                  <span
                    key={g.id}
                    className="bg-muted flex items-center gap-1 rounded-full px-2.5 py-0.5 text-xs"
                  >
                    {g.name}
                    <button
                      onClick={() =>
                        update({
                          genreIds: filters.genreIds?.filter(
                            (id) => id !== g.id,
                          ),
                        })
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
              <span className="text-xs font-medium">
                {filters.radius ?? 50} km
              </span>
            </div>
            <Slider
              min={1}
              max={200}
              step={1}
              defaultValue={[filters.radius ?? 50]}
              onValueChange={([v]) => update({ radius: v })}
            />
          </div>

          <div className="flex gap-2">
            <Select
              value={filters.orderBy ?? ""}
              onValueChange={(v) => update({ orderBy: v })}
            >
              <SelectTrigger className="flex-1">
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
              <SelectTrigger className="flex-1">
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
              <input
                type="checkbox"
                checked={filters.showHistory ?? false}
                onChange={(e) => update({ showHistory: e.target.checked })}
              />
              Show History
            </label>
            <label className="flex cursor-pointer items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={filters.showSold ?? false}
                onChange={(e) => update({ showSold: e.target.checked })}
              />
              Show Sold
            </label>
          </div>

          <Button className="w-full" onClick={apply}>
            Apply
          </Button>
        </div>
      </SheetContent>
    </Sheet>
  );
}
