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
      { rootMargin: "-20% 0px -60% 0px", threshold: 0 },
    );

    sections.forEach(({ id }) => {
      const el = document.getElementById(id);
      if (el) observerRef.current?.observe(el);
    });

    return () => observerRef.current?.disconnect();
  }, [sections]);

  function scrollTo(id: string) {
    document
      .getElementById(id)
      ?.scrollIntoView({ behavior: "smooth", block: "start" });
  }

  return (
    <nav
      className="bg-background border-border sticky z-10 border-b"
      style={{ top: totalHeight }}
    >
      <div className="mx-auto max-w-6xl px-6">
        <ul className="flex justify-center gap-6">
          {sections.map(({ id, label }) => (
            <li key={id}>
              <button
                onClick={() => scrollTo(id)}
                className={`border-b-2 py-3 text-sm font-medium transition-colors ${
                  activeId === id
                    ? "border-foreground text-foreground"
                    : "text-muted-foreground hover:text-foreground border-transparent"
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
