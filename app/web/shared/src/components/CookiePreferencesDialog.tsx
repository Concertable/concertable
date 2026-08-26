import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import {
  CONSENT_CATEGORIES,
  DENIED_DECISION,
  type ConsentCategory,
  type ConsentDecision,
} from "@/lib/consent";
import { useConsent } from "@/providers/ConsentProvider";

const CATEGORY_META: Record<
  ConsentCategory,
  { label: string; description: string }
> = {
  analytics: {
    label: "Analytics",
    description: "Help us understand how the site is used so we can improve it.",
  },
  marketing: {
    label: "Marketing",
    description: "Allow personalised marketing and campaign measurement.",
  },
};

export function CookiePreferencesDialog() {
  const { record, preferencesOpen, closePreferences, save, acceptAll, rejectAll } =
    useConsent();
  const [decision, setDecision] = useState<ConsentDecision>(
    () => record?.categories ?? DENIED_DECISION,
  );

  useEffect(() => {
    if (preferencesOpen) setDecision(record?.categories ?? DENIED_DECISION);
  }, [preferencesOpen, record]);

  return (
    <Dialog
      open={preferencesOpen}
      onOpenChange={(open) => {
        if (!open) closePreferences();
      }}
    >
      <DialogContent data-testid="cookie-prefs">
        <DialogHeader>
          <DialogTitle>Cookie preferences</DialogTitle>
          <DialogDescription>
            Choose which optional cookies Concertable may use. Strictly necessary
            cookies are always on. Read our <a href="/cookies">cookie policy</a>.
          </DialogDescription>
        </DialogHeader>

        <div className="flex flex-col gap-4 py-1">
          <div className="flex items-start justify-between gap-4">
            <div className="flex flex-col gap-1">
              <Label>Strictly necessary</Label>
              <p className="text-muted-foreground text-xs">
                Required to sign in and run the site. Always on.
              </p>
            </div>
            <Checkbox checked disabled aria-label="Strictly necessary (always on)" />
          </div>
          {CONSENT_CATEGORIES.map((category) => (
            <div
              key={category}
              className="flex items-start justify-between gap-4"
            >
              <div className="flex flex-col gap-1">
                <Label htmlFor={`cookie-cat-${category}`}>
                  {CATEGORY_META[category].label}
                </Label>
                <p className="text-muted-foreground text-xs">
                  {CATEGORY_META[category].description}
                </p>
              </div>
              <Checkbox
                id={`cookie-cat-${category}`}
                data-testid={`cookie-cat-${category}`}
                checked={decision[category]}
                onCheckedChange={(checked) =>
                  setDecision((prev) => ({ ...prev, [category]: checked === true }))
                }
              />
            </div>
          ))}
        </div>

        <Separator />

        <DialogFooter>
          <Button variant="ghost" onClick={rejectAll}>
            Reject all
          </Button>
          <Button variant="ghost" onClick={acceptAll}>
            Accept all
          </Button>
          <Button data-testid="cookie-save-prefs" onClick={() => save(decision)}>
            Save preferences
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
