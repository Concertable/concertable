import type { Concert } from "@/types/concert";
import { Hero } from "@/components/Hero";
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

      <ScrollspyNav sections={SECTIONS} />

      <div className="max-w-6xl mx-auto px-6 py-10">
        <div className="flex gap-10">
          <div className="flex-1 space-y-10">
            <section id="about" className="space-y-2 scroll-mt-24">
              <h2 className="text-xl font-semibold">About</h2>
              <EditableTextarea onChange={onAboutChange} placeholder="Tell people about this concert...">
                {concert.about}
              </EditableTextarea>
              {Array.from({ length: 6 }).map((_, i) => (
                <p key={i} className="text-muted-foreground text-sm leading-relaxed">
                  Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
                </p>
              ))}
            </section>

            <div className="border-t border-border" />

            <section id="artist" className="space-y-2 scroll-mt-24">
              <h2 className="text-xl font-semibold">Artist</h2>
              <p className="text-muted-foreground">{concert.artist.name}</p>
              {Array.from({ length: 6 }).map((_, i) => (
                <p key={i} className="text-muted-foreground text-sm leading-relaxed">
                  Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
                </p>
              ))}
            </section>

            <div className="border-t border-border" />

            <section id="venue" className="space-y-2 scroll-mt-24">
              <h2 className="text-xl font-semibold">Venue</h2>
              <p className="text-muted-foreground">{concert.venue.name}</p>
              {Array.from({ length: 6 }).map((_, i) => (
                <p key={i} className="text-muted-foreground text-sm leading-relaxed">
                  Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
                </p>
              ))}
            </section>

            <div className="border-t border-border" />

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
