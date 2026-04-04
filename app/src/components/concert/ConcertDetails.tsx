import type { Concert } from "@/types/concert";
import { Hero } from "@/components/Hero";
import { GoogleMap } from "@/components/GoogleMap";
import { EditableTextarea } from "@/components/editable/EditableTextarea";
import { ReviewSection } from "@/components/reviews/ReviewSection";
import { ConcertCard } from "@/components/concert/ConcertCard";
import { ScrollspyNav } from "@/components/ScrollspyNav";

const SECTIONS = [
  { id: "about", label: "About" },
  { id: "artist", label: "Artist" },
  { id: "venue", label: "Venue" },
  { id: "reviews", label: "Reviews" },
];

interface Props {
  concert: Concert;
  onNameChange?: (value: string) => void;
  onAboutChange?: (value: string) => void;
}

export function ConcertDetails({
  concert,
  onNameChange,
  onAboutChange,
}: Readonly<Props>) {
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

      <ScrollspyNav sections={SECTIONS} />

      <div className="mx-auto max-w-6xl px-6 py-10">
        <div className="flex gap-10">
          <div className="flex-1 space-y-10">
            <section id="about" className="scroll-mt-24 space-y-2">
              <h2 className="text-xl font-semibold">About</h2>
              <EditableTextarea
                onChange={onAboutChange}
                placeholder="Tell people about this concert..."
              >
                {concert.about}
              </EditableTextarea>
            </section>

            <div className="border-border border-t" />

            <section id="artist" className="scroll-mt-24 space-y-2">
              <h2 className="text-xl font-semibold">Artist</h2>
              <p className="text-muted-foreground">{concert.artist.name}</p>
            </section>

            <div className="border-border border-t" />

            <section id="venue" className="scroll-mt-24 space-y-3">
              <h2 className="text-xl font-semibold">Venue</h2>
              <p className="text-muted-foreground">{concert.venue.name}</p>
              <GoogleMap
                className="mt-3"
                lat={concert.venue.latitude}
                lng={concert.venue.longitude}
              />
            </section>

            <div className="border-border border-t" />

            <section id="reviews" className="scroll-mt-24">
              <ReviewSection type="concert" id={concert.id} />
            </section>
          </div>

          <div className="w-72 shrink-0">
            <div className="sticky top-28">
              <ConcertCard concert={concert} />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
