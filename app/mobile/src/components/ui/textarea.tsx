import { cn } from "@/lib/utils";
import { Platform, TextInput } from "react-native";

function Textarea({
  className,
  numberOfLines = 4,
  ...props
}: React.ComponentProps<typeof TextInput> & React.RefAttributes<TextInput>) {
  return (
    <TextInput
      multiline
      numberOfLines={numberOfLines}
      textAlignVertical="top"
      className={cn(
        "dark:bg-input/30 border-input bg-background text-foreground w-full min-w-0 rounded-md border px-3 py-2 text-base leading-5 shadow-sm shadow-black/5",
        props.editable === false &&
          cn(
            "opacity-50",
            Platform.select({
              web: "disabled:pointer-events-none disabled:cursor-not-allowed",
            }),
          ),
        Platform.select({
          web: cn(
            "placeholder:text-muted-foreground selection:bg-primary selection:text-primary-foreground outline-none transition-[color,box-shadow] md:text-sm",
            "focus-visible:border-ring focus-visible:ring-ring/50 focus-visible:ring-[3px]",
          ),
          native: "placeholder:text-muted-foreground/50",
        }),
        className,
      )}
      style={{ minHeight: 96 }}
      {...props}
    />
  );
}

export { Textarea };
