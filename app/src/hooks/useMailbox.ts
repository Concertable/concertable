import { useState } from "react";
import { useUnreadCountQuery, useMessagesQuery } from "@/hooks/query/useMessageQuery";
import { usePagination } from "@/hooks/usePagination";

export function useMailbox() {
  const [open, setOpen] = useState(false);
  const { params, nextPage, prevPage } = usePagination();

  const { data: unreadCount } = useUnreadCountQuery();
  const { data: messages, isLoading, isError } = useMessagesQuery(params, open);

  return {
    open,
    setOpen,
    unreadCount: unreadCount ?? 0,
    messages,
    isLoading,
    isError,
    params,
    nextPage,
    prevPage,
  };
}
