import { ConfigBar } from "@/components/ConfigBar";
import { EditableProvider } from "@concertable/shared/providers";
import { DetailsPageSkeleton } from "@/components/skeletons/DetailsPageSkeleton";
import { ReviewSection } from "@/features/reviews";
import { useMyArtist } from "../hooks/useMyArtist";
import { useArtistStore } from "../store/useArtistStore";
import { ArtistDetails } from "../components/ArtistDetails";

const SECTIONS = [
  { id: "about", label: "About" },
  { id: "location", label: "Location" },
  { id: "concerts", label: "Concerts" },
  { id: "opportunities", label: "Opportunities" },
  { id: "reviews", label: "Reviews" },
];

export function MyArtistPage() {
  const { artist, isDirty, isSaving, save, resetDraft, toggleEdit, editMode } =
    useMyArtist();

  const draft = useArtistStore((state) => state.draft);
  const setName = useArtistStore((state) => state.setName);
  const setAbout = useArtistStore((state) => state.setAbout);

  if (!artist) return <DetailsPageSkeleton sections={5} />;

  const display = draft ?? artist;

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
        <ArtistDetails
          artist={display}
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
            <p className="text-muted-foreground">No opportunities yet.</p>
          </section>

          <div className="border-border border-t" />

          <section id="reviews" className="scroll-mt-24">
            <ReviewSection type="artist" id={display.id} />
          </section>
        </ArtistDetails>
      </EditableProvider>
    </div>
  );
}
