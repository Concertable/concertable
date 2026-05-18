import type { FixturePersona } from "@concertable/shared/features/dashboard";

const personas: FixturePersona[] = ["empty", "mid", "thriving"];

function applyPersona(persona: FixturePersona) {
  const url = new URL(window.location.href);
  url.searchParams.set("persona", persona);
  window.location.href = url.toString();
}

function currentPersona(): FixturePersona {
  if (typeof window === "undefined") return "mid";
  const raw = new URLSearchParams(window.location.search).get("persona");
  return (personas as string[]).includes(raw ?? "") ? (raw as FixturePersona) : "mid";
}

export function PersonaSwitcher() {
  const current = currentPersona();

  return (
    <div className="fixed right-4 bottom-4 z-50 flex items-center gap-1 rounded-full border border-amber-300 bg-amber-50 px-2 py-1 shadow-lg">
      <span className="px-1 text-[10px] font-semibold uppercase tracking-wide text-amber-700">
        mock
      </span>
      {personas.map((p) => (
        <button
          key={p}
          type="button"
          onClick={() => applyPersona(p)}
          className={
            p === current
              ? "rounded-full bg-amber-600 px-2 py-0.5 text-xs font-medium text-white"
              : "rounded-full px-2 py-0.5 text-xs font-medium text-amber-700 hover:bg-amber-100"
          }
        >
          {p}
        </button>
      ))}
    </div>
  );
}
