import { MailIcon } from "lucide-react";
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { Button } from "@/components/ui/button";
import { PaginationControls } from "@/components/ui/PaginationControls";
import { useMailbox } from "@/hooks/useMailbox";

export function Mailbox() {
  const { open, setOpen, unreadCount, messages, isLoading, isError, params, nextPage, prevPage } = useMailbox();

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger asChild>
        <Button variant="ghost" size="icon" className="relative">
          <MailIcon />
          {unreadCount > 0 && (
            <span className="absolute -top-0.5 -right-0.5 flex size-4 items-center justify-center rounded-full bg-primary text-[10px] font-medium text-primary-foreground">
              {unreadCount > 99 ? "99+" : unreadCount}
            </span>
          )}
        </Button>
      </SheetTrigger>

      <SheetContent className="flex flex-col">
        <SheetHeader>
          <SheetTitle>Inbox</SheetTitle>
        </SheetHeader>

        <div className="flex-1 overflow-y-auto mt-4 space-y-2">
          {isLoading && <p className="text-sm text-muted-foreground">Loading messages...</p>}
          {isError && <p className="text-sm text-destructive">Failed to load messages.</p>}
          {!isLoading && !messages?.data.length && (
            <p className="text-sm text-muted-foreground">No messages yet.</p>
          )}
          {messages?.data.map((message) => (
            <div key={message.id} className="rounded-lg border border-border p-3 space-y-1">
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

        <PaginationControls
          pageNumber={params.pageNumber}
          totalPages={messages?.totalPages ?? 0}
          onPrev={prevPage}
          onNext={nextPage}
        />
      </SheetContent>
    </Sheet>
  );
}
