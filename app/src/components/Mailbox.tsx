import { MailIcon } from "lucide-react";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Button } from "@/components/ui/button";
import { PaginationControls } from "@/components/ui/PaginationControls";
import { useMailbox } from "@/hooks/useMailbox";

export function Mailbox() {
  const {
    open,
    setOpen,
    unreadCount,
    messages,
    isLoading,
    isError,
    params,
    nextPage,
    prevPage,
  } = useMailbox();

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button variant="ghost" size="icon" className="relative">
          <MailIcon />
          {unreadCount > 0 && (
            <span className="bg-primary text-primary-foreground absolute -top-0.5 -right-0.5 flex size-4 items-center justify-center rounded-full text-[10px] font-medium">
              {unreadCount > 99 ? "99+" : unreadCount}
            </span>
          )}
        </Button>
      </PopoverTrigger>

      <PopoverContent align="end" className="w-80 p-0">
        <div className="bg-secondary border-border border-b px-3 py-2">
          <p className="text-secondary-foreground text-sm font-medium">Inbox</p>
        </div>

        <div className="divide-border max-h-96 divide-y overflow-y-auto">
          {isLoading && (
            <p className="text-muted-foreground p-3 text-sm">
              Loading messages...
            </p>
          )}
          {isError && (
            <p className="text-destructive p-3 text-sm">
              Failed to load messages.
            </p>
          )}
          {!isLoading && !messages?.data.length && (
            <p className="text-muted-foreground p-3 text-sm">
              No messages yet.
            </p>
          )}
          {messages?.data.map((message) => (
            <div key={message.id} className="space-y-1 px-3 py-2.5">
              <div className="flex items-center justify-between gap-2">
                <span className="text-muted-foreground text-xs">
                  {message.fromUser.email}
                </span>
                {message.action && (
                  <span className="bg-muted text-muted-foreground rounded-full px-2 py-0.5 text-xs">
                    {message.action}
                  </span>
                )}
              </div>
              <p className="text-sm">{message.content}</p>
            </div>
          ))}
        </div>

        {(messages?.totalPages ?? 0) > 1 && (
          <div className="border-border border-t px-3 py-2">
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
