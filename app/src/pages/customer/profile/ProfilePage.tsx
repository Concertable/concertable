import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { CheckCircle, XCircle } from "lucide-react";
import { useAuthStore } from "@/store/useAuthStore";
import { sendVerificationEmail, changePassword } from "@/api/authApi";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";

const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, "Current password is required"),
    newPassword: z.string().min(8, "Password must be at least 8 characters"),
    confirmPassword: z.string().min(1, "Please confirm your new password"),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

type ChangePasswordForm = z.infer<typeof changePasswordSchema>;

export default function ProfilePage() {
  const user = useAuthStore((s) => s.user);
  const [resending, setResending] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ChangePasswordForm>({
    resolver: zodResolver(changePasswordSchema),
  });

  async function handleResendVerification() {
    if (!user) return;
    setResending(true);
    try {
      await sendVerificationEmail(user.email);
      toast.success("Verification email sent");
    } catch {
      toast.error("Failed to send verification email");
    } finally {
      setResending(false);
    }
  }

  async function onSubmit(data: ChangePasswordForm) {
    try {
      await changePassword(data.currentPassword, data.newPassword);
      toast.success("Password changed successfully");
      reset();
    } catch {
      toast.error("Current password is incorrect");
    }
  }

  if (!user) return null;

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
              <Button
                size="sm"
                onClick={handleResendVerification}
                disabled={resending}
                className="ml-2"
              >
                {resending ? "Sending..." : "Resend verification email"}
              </Button>
            </>
          )}
        </div>
      </div>

      <Separator />

      <div className="space-y-4">
        <h3 className="font-medium">Change Password</h3>
        <form
          onSubmit={(e) => {
            void handleSubmit(onSubmit)(e);
          }}
          className="space-y-4"
        >
          <div className="space-y-1">
            <Label htmlFor="currentPassword">Current password</Label>
            <Input
              id="currentPassword"
              type="password"
              {...register("currentPassword")}
            />
            {errors.currentPassword && (
              <p className="text-destructive text-sm">
                {errors.currentPassword.message}
              </p>
            )}
          </div>

          <div className="space-y-1">
            <Label htmlFor="newPassword">New password</Label>
            <Input
              id="newPassword"
              type="password"
              {...register("newPassword")}
            />
            {errors.newPassword && (
              <p className="text-destructive text-sm">
                {errors.newPassword.message}
              </p>
            )}
          </div>

          <div className="space-y-1">
            <Label htmlFor="confirmPassword">Confirm new password</Label>
            <Input
              id="confirmPassword"
              type="password"
              {...register("confirmPassword")}
            />
            {errors.confirmPassword && (
              <p className="text-destructive text-sm">
                {errors.confirmPassword.message}
              </p>
            )}
          </div>

          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Saving..." : "Change Password"}
          </Button>
        </form>
      </div>
    </div>
  );
}
