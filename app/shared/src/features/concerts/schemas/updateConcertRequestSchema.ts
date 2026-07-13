import { z } from "zod";

// Bounds mirror the backend UpdateConcertRequestValidator — keep them in sync.
export const updateConcertRequestSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Name is required")
    .max(100, "Name must be 100 characters or fewer"),
  about: z.string().max(1000, "About must be 1000 characters or fewer"),
  price: z.number().min(0, "Price can't be negative"),
  totalTickets: z.number().int().min(0, "Ticket count can't be negative"),
});

export type UpdateConcertRequest = z.infer<typeof updateConcertRequestSchema>;
