import { z } from "zod";
import { GENRE_VALUES } from "../../../types/common";

export const SearchSchema = () =>
  z.object({
    headerType: z.enum(["concert", "artist", "venue"]).default("concert"),
    query: z.string().optional(),
    lat: z.number().optional(),
    lng: z.number().optional(),
    locationLabel: z.string().optional(),
    from: z.string().optional(),
    to: z.string().optional(),
    genres: z
      .union([
        z.array(z.enum(GENRE_VALUES)),
        z.enum(GENRE_VALUES).transform((g) => [g]),
      ])
      .optional(),
    radius: z.number().optional(),
    orderBy: z.enum(["name", "date"]).optional(),
    sortOrder: z.enum(["asc", "desc"]).optional(),
    showHistory: z.boolean().optional(),
    showSold: z.boolean().optional(),
  });

export type SearchFilters = z.infer<ReturnType<typeof SearchSchema>>;
