import { Button } from "@/components/ui/button";
import type { ComponentProps } from "react";

export function IconButton({
  children,
  ...props
}: ComponentProps<typeof Button>) {
  return (
    <Button
      variant="ghost"
      {...props}
      className="h-auto w-auto rounded-full p-1"
    >
      {children}
    </Button>
  );
}
