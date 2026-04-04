import { useMyArtist } from "@/hooks/useMyArtist";
import { useArtistStore } from "@/store/useArtistStore";
import { ConfigBar } from "@/components/ConfigBar";
import { ArtistDetails } from "@/components/artist/ArtistDetails";
import { EditableProvider } from "@/providers/EditableProvider";
import { DetailsPageSkeleton } from "@/components/skeletons/DetailsPageSkeleton";

export default function MyArtistPage() {
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
        />
      </EditableProvider>
    </div>
  );
}
