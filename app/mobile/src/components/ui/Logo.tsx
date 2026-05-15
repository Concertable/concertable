import { Image, type ImageStyle, type StyleProp } from "react-native";

interface Props {
  size?: "sm" | "md" | "lg";
  withWordmark?: boolean;
  style?: StyleProp<ImageStyle>;
}

const sizes = {
  sm: { height: 28, aspectRatio: 1 },
  md: { height: 40, aspectRatio: 1 },
  lg: { height: 64, aspectRatio: 1 },
};

const wordmarkSizes = {
  sm: { height: 28, width: 120 },
  md: { height: 40, width: 160 },
  lg: { height: 56, width: 220 },
};

export function Logo({ size = "md", withWordmark = false, style }: Readonly<Props>) {
  if (withWordmark) {
    const { height, width } = wordmarkSizes[size];
    return (
      <Image
        source={require("../../assets/brand/logo-long.png")}
        style={[{ height, width, resizeMode: "contain" }, style]}
      />
    );
  }
  const { height, aspectRatio } = sizes[size];
  return (
    <Image
      source={require("../../assets/brand/logo.png")}
      style={[{ height, aspectRatio, resizeMode: "contain" }, style]}
    />
  );
}
