import { useMyArtist } from "@/hooks/useMyArtist";
import { useArtistStore } from "@/store/useArtistStore";
import { ConfigBar } from "@/components/ConfigBar";
import { ArtistDetails } from "@/components/artist/ArtistDetails";

export default function MyArtistPage() {
  const { artist, isDirty, isSaving, save, resetDraft, toggleEdit, editMode } = useMyArtist();

  const draft = useArtistStore((state) => state.draft);
  const setName = useArtistStore((state) => state.setName);
  const setAbout = useArtistStore((state) => state.setAbout);

  if (!artist) return <div className="p-6 text-muted-foreground">Loading...</div>;

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
      <ArtistDetails
        artist={display}
        editMode={editMode}
        onNameChange={setName}
        onAboutChange={setAbout}
      />
    </div>
  );
}
