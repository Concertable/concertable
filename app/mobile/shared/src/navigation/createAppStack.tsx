import { createNativeStackNavigator } from "@react-navigation/native-stack";
import type { ParamListBase } from "@react-navigation/native";
import type { NativeStackNavigationOptions } from "@react-navigation/native-stack";
import type { ComponentProps } from "react";
import { theme } from "../lib/theme";

const brandedHeaderOptions: NativeStackNavigationOptions = {
  headerStyle: { backgroundColor: theme.primary },
  headerTintColor: theme.primaryForeground,
  headerTitleStyle: { color: theme.primaryForeground },
  headerBackButtonDisplayMode: "minimal",
  headerShadowVisible: false,
};

export function createAppStack<T extends ParamListBase>() {
  const Stack = createNativeStackNavigator<T>();
  type NavigatorProps = ComponentProps<typeof Stack.Navigator>;

  function Navigator({ screenOptions, ...rest }: NavigatorProps) {
    const merged: NavigatorProps["screenOptions"] =
      typeof screenOptions === "function"
        ? (args) => ({ ...brandedHeaderOptions, ...screenOptions(args) })
        : { ...brandedHeaderOptions, ...screenOptions };
    return <Stack.Navigator {...rest} screenOptions={merged} />;
  }

  return { Navigator, Screen: Stack.Screen, Group: Stack.Group };
}
