import {
  useQuery,
  useMutation,
  useQueryClient,
  keepPreviousData,
} from "@tanstack/react-query";
import messageApi from "@/api/messageApi";
import type { PaginationParams } from "@/hooks/usePagination";

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

export function useMarkAsReadMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: messageApi.markAsRead,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["messages"] });
    },
  });
}
