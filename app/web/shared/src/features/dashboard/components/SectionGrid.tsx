import { cn } from "@/lib/utils";

export function SectionGrid({
  className,
  children,
}: {
  className?: string;
  children: React.ReactNode;
}) {
  return (
    <div className={cn("grid grid-cols-12 gap-4 [&>*]:min-w-0", className)}>
      {children}
    </div>
  );
}
