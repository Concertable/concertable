import { Fragment, type ReactNode } from "react";
import { ScrollspyNav, type Section } from "@/components/ScrollspyNav";

export interface DetailsSection extends Section {
  content: ReactNode;
}

interface Props {
  hero: ReactNode;
  sections: DetailsSection[];
}

export function DetailsLayout({ hero, sections }: Readonly<Props>) {
  return (
    <div>
      {hero}
      <ScrollspyNav sections={sections} />
      <div className="mx-auto max-w-4xl space-y-10 px-6 py-10">
        {sections.map((section, i) => (
          <Fragment key={section.id}>
            {i > 0 && <div className="border-border border-t" />}
            <section id={section.id} className="scroll-mt-24">
              {section.content}
            </section>
          </Fragment>
        ))}
      </div>
    </div>
  );
}
