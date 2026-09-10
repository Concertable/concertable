import { z } from "zod";
import type { UpdateConcertRequest } from "../types";

export const updateConcertRequestSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Name is required")
    .max(100, "Name must be 100 characters or fewer"),
  about: z.string().max(1000, "About must be 1000 characters or fewer"),
  price: z.number().min(0, "Price can't be negative"),
  totalTickets: z.number().int().min(0, "Ticket count can't be negative"),
}) satisfies z.ZodType<UpdateConcertRequest>;
