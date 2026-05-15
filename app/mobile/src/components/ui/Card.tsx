import { View, type ViewProps } from "react-native";

interface Props extends ViewProps {
  children: React.ReactNode;
}

export function Card({ children, className, ...props }: Readonly<Props>) {
  return (
    <View
      className={`bg-card rounded-2xl border border-border p-4 ${className ?? ""}`}
      {...props}
    >
      {children}
    </View>
  );
}
