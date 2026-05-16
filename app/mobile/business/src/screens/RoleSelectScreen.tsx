import { Linking, Pressable, View } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Building2, Music } from "lucide-react-native";
import { Logo } from "@/components/ui/Logo";
import { Button } from "@/components/ui/button";
import { Text } from "@/components/ui/text";
import { useLogin } from "shared/auth/useLogin";
import { theme } from "shared/lib/theme";

const BUSINESS_URL = "https://business.concertable.com";

export function RoleSelectScreen() {
  const { top, bottom } = useSafeAreaInsets();
  const { login, signup, loading, error } = useLogin();

  return (
    <View className="flex-1 bg-background">
      <View
        className="bg-primary items-center px-6 pb-10"
        style={{ paddingTop: top + 40 }}
      >
        <Logo size="lg" withWordmark style={{ tintColor: "white" }} />
        <Text className="text-sm text-primary-foreground/70 text-center mt-3 px-4">
          Whether you book shows or play them, this is where the work happens.
        </Text>
      </View>

      <View className="flex-1 px-6 pt-6 gap-4">
        <RoleCard
          icon={<Building2 size={26} color={theme.primary} />}
          title="I manage a venue"
          description="Post opportunities, review applications, run shows and settle with artists."
          onPress={() => signup("venue")}
          disabled={loading}
        />
        <RoleCard
          icon={<Music size={26} color={theme.primary} />}
          title="I represent artists"
          description="List your artists, apply to opportunities, manage bookings and get paid."
          onPress={() => signup("artist")}
          disabled={loading}
        />

        {error && (
          <Text className="text-sm text-destructive text-center mt-2">{error}</Text>
        )}
      </View>

      <View
        className="items-center px-6 gap-2"
        style={{ paddingBottom: bottom + 24 }}
      >
        <Button variant="ghost" onPress={login} disabled={loading}>
          <Text className="text-sm text-muted-foreground">
            Already have an account?{" "}
            <Text className="text-primary font-medium">Sign in</Text>
          </Text>
        </Button>
        <Button variant="ghost" onPress={() => Linking.openURL(BUSINESS_URL)} disabled={loading}>
          <Text className="text-xs text-muted-foreground/70">
            New? Register at concertable.com
          </Text>
        </Button>
      </View>
    </View>
  );
}

interface RoleCardProps {
  icon: React.ReactNode;
  title: string;
  description: string;
  onPress: () => void;
  disabled: boolean;
}

function RoleCard({ icon, title, description, onPress, disabled }: RoleCardProps) {
  return (
    <Pressable
      onPress={onPress}
      disabled={disabled}
      className="bg-card border border-border rounded-2xl px-5 py-5 gap-3 active:border-primary active:opacity-80"
    >
      <View className="flex-row items-center gap-3">
        {icon}
        <Text className="text-base font-semibold text-foreground">{title}</Text>
      </View>
      <Text className="text-sm text-muted-foreground">{description}</Text>
    </Pressable>
  );
}
