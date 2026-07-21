import { z } from "zod";

/* The client's half of an e-signature: the typed full name (required) and an optional drawn image.
   Its presence IS the consent — the server stamps user/time/IP. Never call apply/accept without it;
   the UI gates them behind the signature step (ESignaturePanel). */
export const eSignatureRequestSchema = z.object({
  signatoryName: z
    .string()
    .refine((value) => value.trim().length > 0, "Type your full name to sign"),
  drawnSignatureImage: z.string().optional(),
});

export type ESignatureRequest = z.infer<typeof eSignatureRequestSchema>;
