import { ConfigBar } from "@/components/ConfigBar";
import { EditableProvider } from "@concertable/shared/providers";
import { DetailsPageSkeleton } from "@/components/skeletons/DetailsPageSkeleton";
import { ReviewSection } from "@/features/reviews";
import { OpportunitySection } from "@/features/concerts";
import { useMyVenue, useVenueStore, VenueDetails } from "@/features/venues";

const SECTIONS = [
  { id: "about", label: "About" },
  { id: "location", label: "Location" },
  { id: "concerts", label: "Concerts" },
  { id: "opportunities", label: "Opportunities" },
  { id: "reviews", label: "Reviews" },
];

export function MyVenuePage() {
  const { venue, isDirty, isSaving, save, resetDraft, toggleEdit, editMode } =
    useMyVenue();

  const draft = useVenueStore((state) => state.draft);
  const setName = useVenueStore((state) => state.setName);
  const setAbout = useVenueStore((state) => state.setAbout);

  if (!venue) return <DetailsPageSkeleton sections={5} />;

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
          sections={SECTIONS}
        >
          <div className="border-border border-t" />

          <section id="concerts" className="scroll-mt-24 space-y-2">
            <h2 className="text-xl font-semibold">Concerts</h2>
            <p className="text-muted-foreground">No upcoming concerts.</p>
          </section>

          <div className="border-border border-t" />

          <section id="opportunities" className="scroll-mt-24 space-y-2">
            <h2 className="text-xl font-semibold">Opportunities</h2>
            <OpportunitySection venueId={display.id} />
          </section>

          <div className="border-border border-t" />

          <section id="reviews" className="scroll-mt-24">
            <ReviewSection type="venue" id={display.id} />
          </section>
        </VenueDetails>
      </EditableProvider>
    </div>
  );
}
