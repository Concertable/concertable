import { useState } from "react";
import { usePagination } from "@/hooks/usePagination";
import {
  useUnreadCountQuery,
  useMessagesQuery,
} from "./useMessageQuery";

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
