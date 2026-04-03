import type { Concert } from "@/types/concert";
import { Hero } from "@/components/Hero";
import { EditableTextarea } from "@/components/editable/EditableTextarea";
import { ReviewSection } from "@/components/reviews/ReviewSection";
import { ConcertStickyCard } from "@/components/concert/ConcertStickyCard";

interface Props {
  concert: Concert;
  onNameChange?: (value: string) => void;
  onAboutChange?: (value: string) => void;
}

export function ConcertDetails({ concert, onNameChange, onAboutChange }: Readonly<Props>) {
  return (
    <div>
      <Hero
        imageUrl={concert.artist.imageUrl}
        name={concert.name}
        town={concert.venue.town}
        county={concert.venue.county}
        namePlaceholder="Concert name"
        onNameChange={onNameChange}
      />

      <div className="max-w-6xl mx-auto px-6 py-10">
        <div className="flex gap-10 items-start">
          <div className="flex-1 space-y-10">
            <section className="space-y-2">
              <h2 className="text-xl font-semibold">About</h2>
              <EditableTextarea onChange={onAboutChange} placeholder="Tell people about this concert...">
                {concert.about}
              </EditableTextarea>
            </section>

            <div className="border-t border-border" />

            <section className="space-y-2">
              <h2 className="text-xl font-semibold">Artist</h2>
              <p className="text-muted-foreground">{concert.artist.name}</p>
            </section>

            <div className="border-t border-border" />

            <section className="space-y-2">
              <h2 className="text-xl font-semibold">Venue</h2>
              <p className="text-muted-foreground">{concert.venue.name}</p>
            </section>

            <div className="border-t border-border" />

            <ReviewSection type="concert" id={concert.id} />
          </div>

          <div className="w-72 shrink-0">
            <ConcertStickyCard concert={concert} />
          </div>
        </div>
      </div>
    </div>
  );
}
