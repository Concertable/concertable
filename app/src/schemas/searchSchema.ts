import { z } from "zod";
import type { HeaderType } from "@/types/header";

export const SearchSchema = (defaultHeaderType: HeaderType = "concert") =>
  z.object({
    headerType: z
      .enum(["concert", "artist", "venue"])
      .default(defaultHeaderType),
    query: z.string().optional(),
    lat: z.number().optional(),
    lng: z.number().optional(),
    from: z.string().optional(),
    to: z.string().optional(),
    genreIds: z.preprocess(
      (val) =>
        Array.isArray(val)
          ? val
          : val != null && val !== ""
            ? [val]
            : undefined,
      z.array(z.number()).optional(),
    ),
    radius: z.number().optional(),
    orderBy: z.string().optional(),
    sortOrder: z.enum(["asc", "desc"]).optional(),
    showHistory: z.boolean().optional(),
    showSold: z.boolean().optional(),
  });

export type SearchFilters = z.infer<ReturnType<typeof SearchSchema>>;
