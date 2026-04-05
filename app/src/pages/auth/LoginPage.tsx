import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useNavigate, useSearch, Link } from "@tanstack/react-router";
import { useAuthStore } from "@/store/useAuthStore";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";

const schema = z.object({
  email: z.email(),
  password: z.string().min(1, "Password is required"),
});

type FormData = z.infer<typeof schema>;

export default function LoginPage() {
  const login = useAuthStore((s) => s.login);
  const navigate = useNavigate();
  const { redirect } = useSearch({ strict: false }) as { redirect?: string };

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({ resolver: zodResolver(schema) });

  const onSubmit = async (data: FormData) => {
    try {
      const baseUrl = await login(data);
      await navigate({ to: redirect || baseUrl });
    } catch {
      toast.error("Invalid email or password");
    }
  };

  return (
    <div className="bg-background flex min-h-screen items-center justify-center">
      <div className="w-full max-w-sm px-6">
        <form
          onSubmit={(e) => {
            void handleSubmit(onSubmit)(e);
          }}
          className="space-y-4"
        >
          <h1>Login</h1>

          <div className="space-y-1">
            <Label htmlFor="email">Email</Label>
            <Input id="email" type="email" {...register("email")} />
            {errors.email && (
              <p className="text-destructive text-sm">{errors.email.message}</p>
            )}
          </div>

          <div className="space-y-1">
            <Label htmlFor="password">Password</Label>
            <Input id="password" type="password" {...register("password")} />
            {errors.password && (
              <p className="text-destructive text-sm">
                {errors.password.message}
              </p>
            )}
          </div>

          <Button type="submit" disabled={isSubmitting} className="w-full">
            {isSubmitting ? "Logging in..." : "Login"}
          </Button>

          <p className="text-muted-foreground text-center text-sm">
            No account?{" "}
            <Link
              to="/register"
              className="text-foreground underline underline-offset-4 hover:opacity-70"
            >
              Register
            </Link>
          </p>
        </form>
      </div>
    </div>
  );
}
