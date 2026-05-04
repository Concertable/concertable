import { forwardRef } from "react";
import type { ComponentProps } from "react";
import { Input } from "./input";

type Props = Omit<ComponentProps<typeof Input>, "type">;

export const NumberInput = forwardRef<HTMLInputElement, Props>(
  function NumberInput(props, ref) {
    return (
      <Input
        ref={ref}
        type="number"
        {...props}
        onWheel={(e) => (e.target as HTMLInputElement).blur()}
      />
    );
  },
);
