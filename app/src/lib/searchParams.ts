import { z } from "zod";
import type { HeaderType } from "@/types/header";

export function searchParamsSchema(defaultHeaderType: HeaderType) {
  return z.object({
    headerType: z.enum(["concert", "artist", "venue"]).default(defaultHeaderType),
    query: z.string().optional(),
    lat: z.number().optional(),
    lng: z.number().optional(),
    from: z.string().optional(),
    to: z.string().optional(),
    genreIds: z.preprocess(
      (val) => (val != null ? String(val) : undefined),
      z.string().optional().transform((s) => (s ? s.split(",").map(Number) : undefined)),
    ),
    radius: z.number().optional(),
    orderBy: z.string().optional(),
    sortOrder: z.enum(["asc", "desc"]).optional(),
    showHistory: z.boolean().optional(),
    showSold: z.boolean().optional(),
  });
}

export function validateSearchFilters(defaultHeaderType: HeaderType) {
  return searchParamsSchema(defaultHeaderType);
}
