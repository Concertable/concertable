import { useMyVenue } from "@/hooks/useMyVenue";
import { useVenueStore } from "@/store/useVenueStore";
import { ConfigBar } from "@/components/ConfigBar";
import { EditableText } from "@/components/editable/EditableText";
import { EditableTextarea } from "@/components/editable/EditableTextarea";
import { MapPin, Star } from "lucide-react";

export default function MyVenuePage() {
  const { venue, isDirty, isSaving, save, resetDraft, toggleEdit, editMode } = useMyVenue();

  const draft = useVenueStore((state) => state.draft);
  const setName = useVenueStore((state) => state.setName);
  const setAbout = useVenueStore((state) => state.setAbout);

  if (!venue) return <div className="p-6 text-muted-foreground">Loading...</div>;

  const display = draft ?? venue;

  return (
    <div>
      <ConfigBar
        editMode={editMode}
        isDirty={isDirty}
        isSaving={isSaving}
        onToggleEdit={toggleEdit}
        onSave={() => save()}
        onCancel={resetDraft}
      />

      {/* Hero */}
      <div className="relative bg-muted h-72 flex items-end">
        {display.imageUrl && (
          <img
            src={display.imageUrl}
            alt={display.name}
            className="absolute inset-0 w-full h-full object-cover opacity-60"
          />
        )}
        <div className="relative z-10 flex items-end justify-between w-full px-8 pb-6">
          <div className="space-y-1">
            <EditableText
              value={display.name}
              onChange={setName}
              editMode={editMode}
              as="h1"
              placeholder="Venue name"
              className="text-3xl font-bold text-white drop-shadow"
            />
            {(display.town || display.county) && (
              <p className="flex items-center gap-1 text-white/80 text-sm drop-shadow">
                <MapPin className="size-4" />
                {[display.town, display.county].filter(Boolean).join(", ")}
              </p>
            )}
          </div>
          <div className="flex items-center gap-1 text-white/80 text-sm drop-shadow">
            <Star className="size-4 fill-yellow-400 text-yellow-400" />
            <span>No reviews yet</span>
          </div>
        </div>
      </div>

      {/* Sections */}
      <div className="max-w-4xl mx-auto px-6 py-10 space-y-10">
        <section className="space-y-2">
          <h2 className="text-xl font-semibold">About</h2>
          <EditableTextarea
            value={display.about}
            onChange={setAbout}
            editMode={editMode}
            placeholder="Tell artists about your venue..."
          />
        </section>

        <div className="border-t border-border" />

        <section className="space-y-2">
          <h2 className="text-xl font-semibold">Location</h2>
          <p className="flex items-center gap-2 text-muted-foreground">
            <MapPin className="size-4" />
            {[display.town, display.county].filter(Boolean).join(", ") || "No location set."}
          </p>
        </section>

        <div className="border-t border-border" />

        <section className="space-y-2">
          <h2 className="text-xl font-semibold">Events</h2>
          <p className="text-muted-foreground">No upcoming events.</p>
        </section>

        <div className="border-t border-border" />

        <section className="space-y-2">
          <h2 className="text-xl font-semibold">Listings</h2>
          <p className="text-muted-foreground">No listings yet.</p>
        </section>
      </div>
    </div>
  );
}
