interface Props {
  pageNumber: number;
  totalPages: number;
  onPrev: () => void;
  onNext: () => void;
}

export function PaginationControls({
  pageNumber,
  totalPages,
  onPrev,
  onNext,
}: Readonly<Props>) {
  if (totalPages <= 1) return null;

  return (
    <div className="flex items-center justify-end gap-2 text-sm">
      <button
        onClick={onPrev}
        disabled={pageNumber === 1}
        className="border-border rounded border px-3 py-1 disabled:opacity-40"
      >
        Previous
      </button>
      <span className="text-muted-foreground">
        {pageNumber} / {totalPages}
      </span>
      <button
        onClick={onNext}
        disabled={pageNumber === totalPages}
        className="border-border rounded border px-3 py-1 disabled:opacity-40"
      >
        Next
      </button>
    </div>
  );
}
