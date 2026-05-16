import Toast from "react-native-toast-message";

export function notify(message: string, type: "success" | "error" | "info" = "info") {
  Toast.show({ type, text1: message });
}
