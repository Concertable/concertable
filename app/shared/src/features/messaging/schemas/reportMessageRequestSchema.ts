import { z } from "zod";

// Bounds mirror the backend ReportMessageRequestValidator — keep them in sync. Details stay optional:
// the category may say everything, and a reporting route must never be harder to complete than it has
// to be.
export const reportMessageRequestSchema = z.object({
  category: z.enum([
    "IllegalContent",
    "Harassment",
    "Fraud",
    "Spam",
    "Other",
  ]),
  details: z
    .string()
    .max(2000, "Details must be 2000 characters or fewer")
    .optional(),
});

export type ReportMessageRequest = z.infer<typeof reportMessageRequestSchema>;
export type ReportCategory = ReportMessageRequest["category"];
