import { createContext, useContext } from "react";

interface NavbarHeightContextValue {
  navbarHeight: number;
  totalHeight: number;
  setConfigHeight: (height: number) => void;
}

export const NavbarHeightContext = createContext<NavbarHeightContextValue>({
  navbarHeight: 0,
  totalHeight: 0,
  setConfigHeight: () => {},
});

export function useNavbarHeight() {
  return useContext(NavbarHeightContext);
}
