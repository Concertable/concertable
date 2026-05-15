import { ActivityIndicator, Pressable, StyleSheet, Text, View } from "react-native";
import { useLogin } from "../../../auth/useLogin";

export function LoginScreen() {
  const { login, loading, error } = useLogin();

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Concertable</Text>
      {error && <Text style={styles.error}>{error}</Text>}
      <Pressable
        style={[styles.button, loading && styles.buttonDisabled]}
        onPress={login}
        disabled={loading}
      >
        {loading ? (
          <ActivityIndicator color="#fff" />
        ) : (
          <Text style={styles.buttonText}>Sign in</Text>
        )}
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, alignItems: "center", justifyContent: "center", padding: 24 },
  title: { fontSize: 32, fontWeight: "700", marginBottom: 48 },
  error: { color: "#e53e3e", marginBottom: 16, textAlign: "center" },
  button: {
    backgroundColor: "#1a1a1a",
    paddingVertical: 14,
    paddingHorizontal: 48,
    borderRadius: 8,
    minWidth: 180,
    alignItems: "center",
  },
  buttonDisabled: { opacity: 0.5 },
  buttonText: { color: "#fff", fontSize: 16, fontWeight: "600" },
});
