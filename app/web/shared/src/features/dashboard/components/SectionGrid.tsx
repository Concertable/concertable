import { cn } from "@/lib/utils";

export function SectionGrid({
  className,
  children,
}: {
  className?: string;
  children: React.ReactNode;
}) {
  return (
    <div className={cn("grid grid-cols-12 gap-4", className)}>{children}</div>
  );
}
