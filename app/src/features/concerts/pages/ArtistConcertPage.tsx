import { getRouteApi } from "@tanstack/react-router";
import { ConfigBar } from "@/components/ConfigBar";
import { EditableProvider } from "@/providers/EditableProvider";
import { DetailsPageSkeleton } from "@/components/skeletons/DetailsPageSkeleton";
import { useMyConcert } from "../hooks/useMyConcert";
import { useConcertStore } from "../store/useConcertStore";
import { ConcertDetails } from "../components/ConcertDetails";

const routeApi = getRouteApi("/artist/my/concerts/concert/$id");

export function ArtistConcertPage() {
  const { id } = routeApi.useParams();
  const { concert, isDirty, isSaving, save, resetDraft, toggleEdit, editMode } =
    useMyConcert(Number(id));

  const draft = useConcertStore((state) => state.draft);
  const setName = useConcertStore((state) => state.setName);
  const setAbout = useConcertStore((state) => state.setAbout);

  if (!concert) return <DetailsPageSkeleton sections={4} />;

  const display = draft ?? concert;

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
        <ConcertDetails
          concert={display}
          onNameChange={setName}
          onAboutChange={setAbout}
        />
      </EditableProvider>
    </div>
  );
}
