import { createContext, useContext } from "react";

export const NavbarHeightContext = createContext<number>(0);

export function useNavbarHeight() {
  return useContext(NavbarHeightContext);
}
