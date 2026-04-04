import { useEffect, useRef, useState } from "react";
import { useNavbarHeight } from "@/context/NavbarHeightContext";

interface Section {
  id: string;
  label: string;
}

interface Props {
  sections: Section[];
}

export function ScrollspyNav({ sections }: Readonly<Props>) {
  const [activeId, setActiveId] = useState(sections[0]?.id);
  const { totalHeight } = useNavbarHeight();
  const observerRef = useRef<IntersectionObserver | null>(null);

  useEffect(() => {
    observerRef.current = new IntersectionObserver(
      (entries) => {
        const visible = entries
          .filter((e) => e.isIntersecting)
          .sort((a, b) => a.boundingClientRect.top - b.boundingClientRect.top);

        if (visible.length > 0) setActiveId(visible[0].target.id);
      },
      { rootMargin: "-20% 0px -60% 0px", threshold: 0 }
    );

    sections.forEach(({ id }) => {
      const el = document.getElementById(id);
      if (el) observerRef.current?.observe(el);
    });

    return () => observerRef.current?.disconnect();
  }, [sections]);

  function scrollTo(id: string) {
    document.getElementById(id)?.scrollIntoView({ behavior: "smooth", block: "start" });
  }

  return (
    <nav className="sticky z-10 bg-background border-b border-border" style={{ top: totalHeight }}>
      <div className="max-w-6xl mx-auto px-6">
        <ul className="flex justify-center gap-6">
          {sections.map(({ id, label }) => (
            <li key={id}>
              <button
                onClick={() => scrollTo(id)}
                className={`py-3 text-sm font-medium border-b-2 transition-colors ${
                  activeId === id
                    ? "border-foreground text-foreground"
                    : "border-transparent text-muted-foreground hover:text-foreground"
                }`}
              >
                {label}
              </button>
            </li>
          ))}
        </ul>
      </div>
    </nav>
  );
}
