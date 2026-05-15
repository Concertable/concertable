import { useEffect, useRef } from "react";
import { Animated } from "react-native";

interface Props {
  width?: number | `${number}%`;
  height?: number;
  rounded?: boolean;
  className?: string;
}

export function Skeleton({ width, height = 16, rounded, className }: Readonly<Props>) {
  const opacity = useRef(new Animated.Value(1)).current;

  useEffect(() => {
    const anim = Animated.loop(
      Animated.sequence([
        Animated.timing(opacity, { toValue: 0.3, duration: 800, useNativeDriver: true }),
        Animated.timing(opacity, { toValue: 1, duration: 800, useNativeDriver: true }),
      ]),
    );
    anim.start();
    return () => anim.stop();
  }, []);

  return (
    <Animated.View
      style={{ opacity, width, height }}
      className={`bg-muted ${rounded ? "rounded-full" : "rounded-lg"} ${className ?? ""}`}
    />
  );
}
