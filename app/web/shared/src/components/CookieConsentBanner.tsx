import { Button } from "@/components/ui/button";
import { CookiePreferencesDialog } from "@/components/CookiePreferencesDialog";
import { useConsent } from "@/providers/ConsentProvider";

export function CookieConsentBanner() {
  const { isDecided, acceptAll, rejectAll, openPreferences } = useConsent();

  return (
    <>
      {!isDecided && (
        <div
          data-testid="cookie-banner"
          className="bg-popover text-popover-foreground fixed inset-x-0 bottom-0 z-50 border-t"
        >
          <div className="mx-auto flex max-w-6xl flex-col gap-3 px-6 py-4 sm:flex-row sm:items-center sm:justify-between">
            <p className="text-muted-foreground text-sm">
              We use strictly necessary cookies to run Concertable. With your
              consent we'd also use optional cookies to measure usage and improve
              the product. Read our{" "}
              <a
                href="/cookies"
                className="hover:text-foreground underline underline-offset-4"
              >
                cookie policy
              </a>
              .
            </p>
            <div className="flex shrink-0 flex-wrap items-center gap-2">
              <Button
                data-testid="cookie-manage"
                variant="ghost"
                size="sm"
                onClick={openPreferences}
              >
                Manage cookies
              </Button>
              <Button
                data-testid="cookie-reject-all"
                variant="default"
                size="sm"
                onClick={rejectAll}
              >
                Reject all
              </Button>
              <Button
                data-testid="cookie-accept-all"
                variant="default"
                size="sm"
                onClick={acceptAll}
              >
                Accept all
              </Button>
            </div>
          </div>
        </div>
      )}
      <CookiePreferencesDialog />
    </>
  );
}
