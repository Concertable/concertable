import { reportMessageRequestSchema } from "../schemas/reportMessageRequestSchema";
import type { ReportCategory } from "../schemas/reportMessageRequestSchema";
import { useReportMessageMutation } from "./useMessageQuery";

export interface ReportBuffer {
  category?: ReportCategory;
  details?: string;
}

export function useReportMessage(messageId: number) {
  const { mutate, isPending, isSuccess, isError } =
    useReportMessageMutation(messageId);

  const validate = (buffer: ReportBuffer) =>
    reportMessageRequestSchema.safeParse({
      category: buffer.category,
      details: buffer.details?.trim() || undefined,
    });

  const submit = (buffer: ReportBuffer) => {
    const parsed = validate(buffer);
    if (parsed.success) mutate(parsed.data);
    return parsed;
  };

  return { validate, submit, isPending, isSuccess, isError };
}
