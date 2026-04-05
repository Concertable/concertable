import type { Concert } from "@/types/concert";
import { Hero } from "@/components/Hero";
import { GoogleMap } from "@/components/GoogleMap";
import { EditableTextarea } from "@/components/editable/EditableTextarea";
import { ReviewSection } from "@/components/reviews/ReviewSection";
import { ReviewSummaryBadge } from "@/components/reviews/ReviewSummaryBadge";
import { ConcertCard } from "@/components/concert/ConcertCard";
import { ScrollspyNav } from "@/components/ScrollspyNav";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { TriangleAlertIcon } from "lucide-react";
import { AddReview } from "@/components/reviews/AddReview";

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

      {!concert.datePosted && (
        <div className="bg-warning/15 text-warning flex items-center gap-3 px-6 py-3 text-sm">
          <TriangleAlertIcon className="text-warning size-4 shrink-0" />
          This concert has not been posted yet and is not visible to the public.
        </div>
      )}

      <ScrollspyNav sections={SECTIONS} />

      <div className="@container mx-auto max-w-6xl px-6 py-10">
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

            <section id="reviews" className="scroll-mt-24 space-y-4">
              <div className="flex items-center justify-between">
                <h2 className="text-xl font-semibold">Reviews</h2>
                <AddReview concertId={concert.id} />
              </div>
              <Tabs defaultValue="artist">
                <div className="flex items-center justify-between gap-6">
                  <TabsList>
                    <TabsTrigger value="artist">Artist</TabsTrigger>
                    <TabsTrigger value="venue">Venue</TabsTrigger>
                    <TabsTrigger value="concert">Concert</TabsTrigger>
                  </TabsList>
                  <TabsContent value="artist">
                    <ReviewSummaryBadge type="artist" id={concert.artist.id} />
                  </TabsContent>
                  <TabsContent value="venue">
                    <ReviewSummaryBadge type="venue" id={concert.venue.id} />
                  </TabsContent>
                  <TabsContent value="concert">
                    <ReviewSummaryBadge type="concert" id={concert.id} />
                  </TabsContent>
                </div>
                <TabsContent value="artist">
                  <ReviewSection type="artist" id={concert.artist.id} />
                </TabsContent>
                <TabsContent value="venue">
                  <ReviewSection type="venue" id={concert.venue.id} />
                </TabsContent>
                <TabsContent value="concert">
                  <ReviewSection type="concert" id={concert.id} />
                </TabsContent>
              </Tabs>
            </section>
          </div>

          <div className="hidden w-72 shrink-0 @3xl:block">
            <div className="sticky top-28">
              <ConcertCard concert={concert} />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
