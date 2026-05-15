import { useEffect, useRef } from "react";
import { Animated, View } from "react-native";

interface SkeletonProps {
  width?: number | `${number}%`;
  height?: number;
  rounded?: boolean;
  className?: string;
}

export function Skeleton({ width, height = 16, rounded, className }: SkeletonProps) {
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
      className={`bg-gray-200 ${rounded ? "rounded-full" : "rounded-lg"} ${className ?? ""}`}
    />
  );
}
