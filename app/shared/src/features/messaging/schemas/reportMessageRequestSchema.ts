import { z } from "zod";
import type { ReportMessageRequest } from "../types";

export const reportMessageRequestSchema = z.object({
  category: z.enum([
    "illegalContent",
    "harassment",
    "fraud",
    "spam",
    "other",
  ]),
  details: z
    .string()
    .trim()
    .max(2000, "Details must be 2000 characters or fewer")
    .transform((details) => details || undefined)
    .optional(),
}) satisfies z.ZodType<ReportMessageRequest>;

export type ReportMessageFormValues = z.input<
  typeof reportMessageRequestSchema
>;
