import type { ReportMessageRequest } from "../types";
import { useReportMessageMutation } from "./useMessageQuery";

export function useReportMessage(messageId: number) {
  const { mutate, isPending, isSuccess, isError } =
    useReportMessageMutation(messageId);

  const submit = (request: ReportMessageRequest) => mutate(request);

  return { submit, isPending, isSuccess, isError };
}
