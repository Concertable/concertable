import { z } from "zod";

// Bounds mirror the backend DoorRevenueRequestValidator — keep them in sync. The external door take
// in pounds (excludes Concertable's own ticket sales, which settlement already knows).
export const doorRevenueRequestSchema = z.object({
  doorRevenue: z.number().min(0, "Door takings can't be negative"),
});

export type DoorRevenueRequest = z.infer<typeof doorRevenueRequestSchema>;
