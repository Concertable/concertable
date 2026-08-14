import {
  useQuery,
  useMutation,
  useQueryClient,
  keepPreviousData,
} from "@tanstack/react-query";
import type { PaginationParams } from "../../../hooks/usePagination";
import messageApi from "../api/messageApi";
import type { ReportMessageRequest } from "../types";

export function useUnreadCountQuery() {
  return useQuery({
    queryKey: ["messages", "unread-count"],
    queryFn: messageApi.getUnreadCount,
  });
}

export function useMessagesQuery(params: PaginationParams, enabled = true) {
  return useQuery({
    queryKey: ["messages", params],
    queryFn: () => messageApi.getMessages(params),
    placeholderData: keepPreviousData,
    enabled,
  });
}

export function useReportMessageMutation() {
  return useMutation({
    mutationFn: ({
      messageId,
      request,
    }: {
      messageId: number;
      request: ReportMessageRequest;
    }) => messageApi.reportMessage(messageId, request),
  });
}

export function useMarkInboxReadMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: messageApi.markInboxRead,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["messages"] });
    },
  });
}
