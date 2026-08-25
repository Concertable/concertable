export const GENRE_VALUES = [
  "rock",
  "pop",
  "jazz",
  "hipHop",
  "electronic",
  "indie",
  "dnB",
  "house",
] as const;

export type Genre = (typeof GENRE_VALUES)[number];

export const GENRE_LABELS: Record<Genre, string> = {
  rock: "Rock",
  pop: "Pop",
  jazz: "Jazz",
  hipHop: "Hip-Hop",
  electronic: "Electronic",
  indie: "Indie",
  dnB: "DnB",
  house: "House",
};

export function genreLabel(genre: Genre): string {
  return GENRE_LABELS[genre];
}

export interface Pagination<T> {
  data: T[];
  totalCount: number;
  totalPages: number;
  pageNumber: number;
  pageSize: number;
}

export interface ActionLink {
  href: string;
  method: string;
}
