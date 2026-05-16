import { DefaultTheme, DarkTheme } from "@react-navigation/native";

export const THEME = {
  light: {
    primary: "hsl(268, 75%, 38%)",
    primaryForeground: "hsl(0, 0%, 98%)",
    secondary: "hsl(268, 65%, 55%)",
    secondaryForeground: "hsl(0, 0%, 98%)",
    background: "hsl(0, 0%, 100%)",
    foreground: "hsl(240, 10%, 4%)",
    card: "hsl(0, 0%, 100%)",
    cardForeground: "hsl(240, 10%, 4%)",
    popover: "hsl(0, 0%, 100%)",
    popoverForeground: "hsl(240, 10%, 4%)",
    border: "hsl(0, 0%, 92%)",
    input: "hsl(0, 0%, 92%)",
    muted: "hsl(0, 0%, 92%)",
    mutedForeground: "hsl(240, 4%, 46%)",
    accent: "hsl(0, 0%, 92%)",
    accentForeground: "hsl(240, 6%, 10%)",
    destructive: "hsl(0, 75%, 52%)",
    destructiveForeground: "hsl(0, 0%, 98%)",
    success: "hsl(145, 55%, 40%)",
    successForeground: "hsl(0, 0%, 98%)",
    warning: "hsl(38, 92%, 50%)",
    warningForeground: "hsl(240, 10%, 4%)",
    gold: "hsl(42, 80%, 55%)",
    goldForeground: "hsl(240, 10%, 4%)",
    ring: "hsl(268, 65%, 55%)",
    tertiary: "hsl(268, 25%, 18%)",
    tertiaryForeground: "hsl(268, 15%, 85%)",
  },
  dark: {
    primary: "hsl(268, 75%, 38%)",
    primaryForeground: "hsl(0, 0%, 98%)",
    secondary: "hsl(268, 65%, 55%)",
    secondaryForeground: "hsl(0, 0%, 98%)",
    background: "hsl(0, 0%, 100%)",
    foreground: "hsl(240, 10%, 4%)",
    card: "hsl(0, 0%, 100%)",
    cardForeground: "hsl(240, 10%, 4%)",
    popover: "hsl(0, 0%, 100%)",
    popoverForeground: "hsl(240, 10%, 4%)",
    border: "hsl(0, 0%, 92%)",
    input: "hsl(0, 0%, 92%)",
    muted: "hsl(0, 0%, 92%)",
    mutedForeground: "hsl(240, 4%, 46%)",
    accent: "hsl(0, 0%, 92%)",
    accentForeground: "hsl(240, 6%, 10%)",
    destructive: "hsl(0, 75%, 52%)",
    destructiveForeground: "hsl(0, 0%, 98%)",
    success: "hsl(145, 55%, 40%)",
    successForeground: "hsl(0, 0%, 98%)",
    warning: "hsl(38, 92%, 50%)",
    warningForeground: "hsl(240, 10%, 4%)",
    gold: "hsl(42, 80%, 55%)",
    goldForeground: "hsl(240, 10%, 4%)",
    ring: "hsl(268, 65%, 55%)",
    tertiary: "hsl(268, 25%, 18%)",
    tertiaryForeground: "hsl(268, 15%, 85%)",
  },
} as const;

export const NAV_THEME = {
  light: {
    ...DefaultTheme,
    colors: {
      ...DefaultTheme.colors,
      background: "hsl(0, 0%, 100%)",
      border: "hsl(0, 0%, 92%)",
      card: "hsl(0, 0%, 100%)",
      notification: "hsl(0, 75%, 52%)",
      primary: "hsl(268, 75%, 38%)",
      text: "hsl(240, 10%, 4%)",
    },
  },
  dark: {
    ...DarkTheme,
    colors: {
      ...DarkTheme.colors,
      background: "hsl(0, 0%, 100%)",
      border: "hsl(0, 0%, 92%)",
      card: "hsl(0, 0%, 100%)",
      notification: "hsl(0, 75%, 52%)",
      primary: "hsl(268, 75%, 38%)",
      text: "hsl(240, 10%, 4%)",
    },
  },
};

// Transitional flat alias — remove once all consumers migrate to THEME.light.*
export const theme = THEME.light;

// Stripe appearance API requires hex — HSL strings are rejected.
export const stripeColors = {
  primary: "#5c18aa",      // hsl(268, 75%, 38%)
  background: "#ffffff",   // hsl(0, 0%, 100%)
} as const;
