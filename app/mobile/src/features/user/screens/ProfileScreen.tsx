import { View, Text, Pressable } from "react-native";
import { useAuthStore } from "@concertable/shared/features/auth";
import { Screen } from "../../../components/ui/Screen";
import { Avatar } from "../../../components/ui/Avatar";
import { Button } from "../../../components/ui/Button";
import { useLogout } from "../../../auth/useLogout";

export function ProfileScreen() {
  const user = useAuthStore((s) => s.user);
  const { logout } = useLogout();

  if (!user) return null;

  return (
    <Screen scroll>
      <View className="items-center gap-3 py-6">
        <Avatar
          uri={undefined}
          name={user.email}
          size={80}
        />
        <View className="items-center gap-1">
          <Text className="text-xl font-bold text-gray-900">{user.email}</Text>
          <View className="bg-gray-100 rounded-full px-3 py-1 mt-1">
            <Text className="text-xs text-gray-600 font-medium">{user.role}</Text>
          </View>
        </View>
      </View>

      <View className="gap-2 mt-4">
        <SectionHeader title="Account" />
        <MenuRow label="Edit Profile" onPress={() => {}} />
        <MenuRow label="Location" onPress={() => {}} />
        <MenuRow label="Preferences" onPress={() => {}} />

        <SectionHeader title="Support" />
        <MenuRow label="Help Center" onPress={() => {}} />
        <MenuRow label="Privacy Policy" onPress={() => {}} />

        <View className="mt-6">
          <Button variant="destructive" onPress={logout}>
            Sign Out
          </Button>
        </View>
      </View>
    </Screen>
  );
}

function SectionHeader({ title }: { title: string }) {
  return <Text className="text-xs font-semibold text-gray-400 uppercase tracking-wider mt-4 mb-1">{title}</Text>;
}

function MenuRow({ label, onPress }: { label: string; onPress: () => void }) {
  return (
    <Pressable
      onPress={onPress}
      className="flex-row items-center justify-between bg-white rounded-xl px-4 py-3.5 border border-gray-100"
    >
      <Text className="text-gray-900">{label}</Text>
      <Text className="text-gray-400">›</Text>
    </Pressable>
  );
}
