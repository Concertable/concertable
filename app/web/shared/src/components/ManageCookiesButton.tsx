import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { useConsent } from "@/providers/ConsentProvider";

export function ManageCookiesButton({ className }: { className?: string }) {
  const { openPreferences } = useConsent();
  return (
    <Button
      type="button"
      data-testid="cookie-manage-footer"
      variant="link"
      onClick={openPreferences}
      className={cn("h-auto p-0 text-inherit", className)}
    >
      Manage cookies
    </Button>
  );
}
