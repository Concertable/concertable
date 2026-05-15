import { Linking, Text, View } from "react-native";
import { CheckCircle, XCircle } from "lucide-react-native";
import { useAuthStore } from "@concertable/shared/features/auth";
import { Screen } from "../../../components/ui/Screen";
import { Button } from "../../../components/ui/Button";
import { Avatar } from "../../../components/ui/Avatar";
import { theme } from "../../../lib/theme";
import Config from "../../../lib/config";

export function EditProfileScreen() {
  const user = useAuthStore((s) => s.user);

  if (!user) return null;

  return (
    <Screen scroll>
      <View className="items-center gap-3 py-6">
        <Avatar uri={undefined} name={user.email} size={80} />
        <Text className="text-xl font-bold text-foreground">{user.email}</Text>
      </View>

      <View className="gap-4">
        <View className="bg-card border border-border rounded-xl px-4 py-3.5 gap-1">
          <Text className="text-xs text-muted-foreground">Email address</Text>
          <Text className="text-sm text-foreground">{user.email}</Text>
        </View>

        <View className="bg-card border border-border rounded-xl px-4 py-3.5 flex-row items-center gap-2">
          {user.isEmailVerified ? (
            <>
              <CheckCircle size={16} color={theme.success} />
              <Text className="text-sm text-success font-medium">Email verified</Text>
            </>
          ) : (
            <>
              <XCircle size={16} color={theme.destructive} />
              <Text className="text-sm text-destructive font-medium">Email not verified</Text>
            </>
          )}
        </View>

        <View className="mt-2 gap-2">
          <Text className="text-xs font-semibold text-muted-foreground uppercase tracking-wider px-1">
            Password
          </Text>
          <Text className="text-sm text-muted-foreground px-1">
            Passwords are managed on the secure sign-in service.
          </Text>
          <Button
            variant="outline"
            onPress={() => Linking.openURL(`${Config.authAuthority}/Account/ChangePassword`)}
          >
            Change Password
          </Button>
        </View>
      </View>
    </Screen>
  );
}
