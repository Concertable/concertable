import { MailIcon } from "lucide-react";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { Button } from "@/components/ui/button";
import { PaginationControls } from "@/components/ui/PaginationControls";
import { useMailbox } from "@/hooks/useMailbox";

export function Mailbox() {
  const { open, setOpen, unreadCount, messages, isLoading, isError, params, nextPage, prevPage } = useMailbox();

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button variant="ghost" size="icon" className="relative">
          <MailIcon />
          {unreadCount > 0 && (
            <span className="absolute -top-0.5 -right-0.5 flex size-4 items-center justify-center rounded-full bg-primary text-[10px] font-medium text-primary-foreground">
              {unreadCount > 99 ? "99+" : unreadCount}
            </span>
          )}
        </Button>
      </PopoverTrigger>

      <PopoverContent align="end" className="w-80 p-0">
        <div className="border-b border-border px-3 py-2">
          <p className="font-medium text-sm">Inbox</p>
        </div>

        <div className="max-h-96 overflow-y-auto divide-y divide-border">
          {isLoading && <p className="text-sm text-muted-foreground p-3">Loading messages...</p>}
          {isError && <p className="text-sm text-destructive p-3">Failed to load messages.</p>}
          {!isLoading && !messages?.data.length && (
            <p className="text-sm text-muted-foreground p-3">No messages yet.</p>
          )}
          {messages?.data.map((message) => (
            <div key={message.id} className="px-3 py-2.5 space-y-1">
              <div className="flex items-center justify-between gap-2">
                <span className="text-xs text-muted-foreground">{message.fromUser.email}</span>
                {message.action && (
                  <span className="rounded-full bg-muted px-2 py-0.5 text-xs text-muted-foreground">
                    {message.action}
                  </span>
                )}
              </div>
              <p className="text-sm">{message.content}</p>
            </div>
          ))}
        </div>

        {(messages?.totalPages ?? 0) > 1 && (
          <div className="border-t border-border px-3 py-2">
            <PaginationControls
              pageNumber={params.pageNumber}
              totalPages={messages?.totalPages ?? 0}
              onPrev={prevPage}
              onNext={nextPage}
            />
          </div>
        )}
      </PopoverContent>
    </Popover>
  );
}
