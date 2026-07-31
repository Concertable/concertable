import { useState } from "react";
import { usePagination } from "../../../hooks/usePagination";
import {
  useUnreadCountQuery,
  useMessagesQuery,
  useMarkInboxReadMutation,
} from "./useMessageQuery";

export function useMailbox() {
  const [open, setOpen] = useState(false);
  const { params, nextPage, prevPage } = usePagination();

  const { data: unreadCount } = useUnreadCountQuery();
  const { data: messages, isLoading, isError } = useMessagesQuery(params, open);
  const { mutate: markInboxRead } = useMarkInboxReadMutation();

  // Opening the inbox is the "I've seen it" event — advance this member's read pointer (per-member
  // state) here in the event handler, not reactively in an Effect.
  const openMailbox = (next: boolean) => {
    setOpen(next);
    if (next && unreadCount) markInboxRead();
  };

  return {
    open,
    setOpen: openMailbox,
    unreadCount: unreadCount ?? 0,
    messages,
    isLoading,
    isError,
    params,
    nextPage,
    prevPage,
  };
}
