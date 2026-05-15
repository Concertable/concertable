import { Pressable, Text, View } from "react-native";
import { useNavigation } from "@react-navigation/native";
import type { NativeStackNavigationProp } from "@react-navigation/native-stack";
import { ChevronRight, CheckCircle, XCircle } from "lucide-react-native";
import { useAuthStore } from "@concertable/shared/features/auth";
import { Screen } from "../../../components/ui/Screen";
import { Avatar } from "../../../components/ui/Avatar";
import { Badge } from "../../../components/ui/Badge";
import { Button } from "../../../components/ui/Button";
import { useLogout } from "../../../auth/useLogout";
import { theme } from "../../../lib/theme";
import type { ProfileStackParamList } from "../../../navigation/types";

type ProfileNav = NativeStackNavigationProp<ProfileStackParamList>;

export function ProfileScreen() {
  const nav = useNavigation<ProfileNav>();
  const user = useAuthStore((s) => s.user);
  const { logout } = useLogout();

  if (!user) return null;

  return (
    <Screen scroll>
      <View className="items-center gap-3 py-6">
        <Avatar uri={undefined} name={user.email} size={80} />
        <View className="items-center gap-2">
          <Text className="text-xl font-bold text-foreground">{user.email}</Text>
          <View className="flex-row items-center gap-2">
            {user.role && <Badge variant="secondary">{user.role}</Badge>}
            {user.isEmailVerified ? (
              <View className="flex-row items-center gap-1">
                <CheckCircle size={14} color={theme.success} />
                <Text className="text-xs text-success font-medium">Verified</Text>
              </View>
            ) : (
              <View className="flex-row items-center gap-1">
                <XCircle size={14} color={theme.destructive} />
                <Text className="text-xs text-destructive font-medium">Unverified</Text>
              </View>
            )}
          </View>
        </View>
      </View>

      <View className="gap-1">
        <SectionHeader title="Account" />
        <MenuRow label="Edit Profile" onPress={() => nav.navigate("EditProfile")} />
        <MenuRow label="Location" onPress={() => nav.navigate("Location")} />
        <MenuRow label="Preferences" onPress={() => nav.navigate("Preferences")} />

        <SectionHeader title="Support" />
        <MenuRow label="Help Center" onPress={() => {}} />
        <MenuRow label="Privacy Policy" onPress={() => {}} />

        <View className="mt-6">
          <Button variant="destructive" onPress={logout}>Sign Out</Button>
        </View>
      </View>
    </Screen>
  );
}

function SectionHeader({ title }: { title: string }) {
  return (
    <Text className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mt-5 mb-1 px-1">
      {title}
    </Text>
  );
}

function MenuRow({ label, onPress }: { label: string; onPress: () => void }) {
  return (
    <Pressable
      onPress={onPress}
      className="flex-row items-center justify-between bg-card rounded-xl px-4 py-3.5 border border-border"
    >
      <Text className="text-sm text-foreground">{label}</Text>
      <ChevronRight size={16} color={theme.mutedForeground} />
    </Pressable>
  );
}
