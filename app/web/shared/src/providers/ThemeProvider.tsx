import { createContext, useContext, useEffect, useState } from "react";
import { createClassifiedStorage } from "@/lib/classifiedStorage";

type Theme = "light" | "dark" | "system";

const themeStorage = createClassifiedStorage<Theme>("theme");

interface ThemeContextValue {
  theme: Theme;
  setTheme: (theme: Theme) => void;
}

const ThemeContext = createContext<ThemeContextValue | undefined>(undefined);

function applyTheme(theme: Theme) {
  const isDark =
    theme === "dark" ||
    (theme === "system" && matchMedia("(prefers-color-scheme: dark)").matches);

  document.documentElement.classList.toggle("dark", isDark);
}

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const [theme, setThemeState] = useState<Theme>(
    () => themeStorage.get() ?? "system",
  );

  useEffect(() => {
    applyTheme(theme);

    if (theme === "system") {
      const media = matchMedia("(prefers-color-scheme: dark)");
      const handler = () => applyTheme("system");
      media.addEventListener("change", handler);
      return () => media.removeEventListener("change", handler);
    }
  }, [theme]);

  function setTheme(theme: Theme) {
    themeStorage.set(theme);
    setThemeState(theme);
  }

  return <ThemeContext value={{ theme, setTheme }}>{children}</ThemeContext>;
}

export function useTheme() {
  const ctx = useContext(ThemeContext);
  if (!ctx) throw new Error("useTheme must be used within ThemeProvider");
  return ctx;
}
