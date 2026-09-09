import type { ActionLink } from "@concertable/shared/types/common";

export function visibleActionNames<TAction extends string>(
  actions: Partial<Record<TAction, ActionLink>>,
  labels: Record<TAction, string>,
  include?: (name: TAction) => boolean,
): TAction[] {
  return (Object.keys(labels) as TAction[]).filter(
    (name) => actions[name] !== undefined && (include?.(name) ?? true),
  );
}
