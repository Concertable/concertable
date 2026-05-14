import { Separator } from "@/components/ui/separator";

export function SettingsPage() {
  return (
    <div className="max-w-lg space-y-8">
      <div>
        <h2 className="text-lg font-semibold">Settings</h2>
        <p className="text-muted-foreground text-sm">
          App preferences and appearance
        </p>
      </div>

      <Separator />

      <div className="space-y-4">
        <h3 className="font-medium">Theme</h3>
        <p className="text-muted-foreground text-sm">
          Theme switching coming soon.
        </p>
      </div>
    </div>
  );
}
