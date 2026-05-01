import { CheckCircle, XCircle } from "lucide-react";
import { useAuthStore } from "@/store/useAuthStore";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";

export function ProfilePage() {
  const user = useAuthStore((s) => s.user);

  if (!user) return null;

  const authority = import.meta.env.VITE_AUTH_AUTHORITY;

  return (
    <div className="max-w-lg space-y-8">
      <div>
        <h2 className="text-lg font-semibold">Account</h2>
        <p className="text-muted-foreground text-sm">
          Manage your account details
        </p>
      </div>

      <Separator />

      <div className="space-y-4">
        <h3 className="font-medium">Email</h3>
        <div className="space-y-1">
          <Label>Email address</Label>
          <Input value={user.email} readOnly className="bg-muted" />
        </div>
        <div className="flex items-center gap-2">
          {user.isEmailVerified ? (
            <>
              <CheckCircle className="size-4 text-green-500" />
              <span className="text-sm text-green-600">Verified</span>
            </>
          ) : (
            <>
              <XCircle className="text-destructive size-4" />
              <span className="text-destructive text-sm">Not verified</span>
            </>
          )}
        </div>
      </div>

      <Separator />

      <div className="space-y-4">
        <h3 className="font-medium">Password</h3>
        <p className="text-muted-foreground text-sm">
          Passwords are managed on the secure sign-in service.
        </p>
        <Button asChild>
          <a href={`${authority}/Account/ChangePassword`}>Change password</a>
        </Button>
      </div>
    </div>
  );
}
