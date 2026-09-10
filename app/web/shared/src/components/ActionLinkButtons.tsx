import type { ComponentProps } from "react";
import type { ActionLink } from "@concertable/shared/types/common";
import { Button } from "@/components/ui/button";
import { visibleActionNames } from "@/lib/actionLinks";
import { cn } from "@/lib/utils";

type ButtonProps = ComponentProps<typeof Button>;

interface Props<TAction extends string> {
  actions: Partial<Record<TAction, ActionLink>>;
  labels: Record<TAction, string>;
  variants?: Partial<Record<TAction, ButtonProps["variant"]>>;
  onAction: (name: TAction) => void;
  include?: (name: TAction) => boolean;
  size?: ButtonProps["size"];
  disabled?: boolean;
  className?: string;
}

export function ActionLinkButtons<TAction extends string>({
  actions,
  labels,
  variants,
  onAction,
  include,
  size = "xs",
  disabled,
  className,
}: Readonly<Props<TAction>>) {
  const names = visibleActionNames(actions, labels, include);
  if (names.length === 0) return null;

  return (
    <div className={cn("flex items-center justify-end gap-1", className)}>
      {names.map((name) => (
        <Button
          key={name}
          size={size}
          variant={variants?.[name]}
          disabled={disabled}
          onClick={() => onAction(name)}
        >
          {labels[name]}
        </Button>
      ))}
    </div>
  );
}
