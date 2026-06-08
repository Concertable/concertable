export const GENRE_VALUES = [
  "Rock",
  "Pop",
  "Jazz",
  "HipHop",
  "Electronic",
  "Indie",
  "DnB",
  "House",
] as const;

export type Genre = (typeof GENRE_VALUES)[number];

const GENRE_LABELS: Record<Genre, string> = {
  Rock: "Rock",
  Pop: "Pop",
  Jazz: "Jazz",
  HipHop: "Hip-Hop",
  Electronic: "Electronic",
  Indie: "Indie",
  DnB: "DnB",
  House: "House",
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
