import { useMyVenue } from "@/hooks/useMyVenue";
import { useVenueStore } from "@/store/useVenueStore";
import { ConfigBar } from "@/components/ConfigBar";
import { VenueDetails } from "@/components/venue/VenueDetails";
import { EditableProvider } from "@/providers/EditableProvider";

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

      <EditableProvider editMode={editMode}>
        <VenueDetails
          venue={display}
          onNameChange={setName}
          onAboutChange={setAbout}
        />
      </EditableProvider>
    </div>
  );
}