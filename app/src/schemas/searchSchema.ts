import { z } from "zod";

export const SearchSchema = () =>
  z.object({
    headerType: z.enum(["concert", "artist", "venue"]),
    query: z.string().optional(),
    lat: z.number().optional(),
    lng: z.number().optional(),
    from: z.string().optional(),
    to: z.string().optional(),
    genreIds: z
      .union([
        z.array(z.coerce.number()),
        z.coerce.number().transform((n) => [n]),
      ])
      .optional(),
    radius: z.number().optional(),
    orderBy: z.string().optional(),
    sortOrder: z.enum(["asc", "desc"]).optional(),
    showHistory: z.boolean().optional(),
    showSold: z.boolean().optional(),
  });

export type SearchFilters = z.infer<ReturnType<typeof SearchSchema>>;
