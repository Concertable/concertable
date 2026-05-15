import type { LucideIcon } from "lucide-react-native";
import { theme } from "../../lib/theme";

interface Props {
  icon: LucideIcon;
  size?: number;
  color?: string;
  strokeWidth?: number;
}

export function Icon({ icon: LucideComponent, size = 20, color = theme.foreground, strokeWidth = 2 }: Readonly<Props>) {
  return <LucideComponent size={size} color={color} strokeWidth={strokeWidth} />;
}
