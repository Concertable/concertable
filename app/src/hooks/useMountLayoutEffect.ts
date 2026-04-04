import { useLayoutEffect } from "react";

export function useMountLayoutEffect(effect: () => void | (() => void)) {
  // eslint-disable-next-line react-hooks/exhaustive-deps
  useLayoutEffect(effect, []);
}
