import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useNavigate, Link } from "@tanstack/react-router";
import { useAuthStore } from "@/store/useAuthStore";
import type { Role } from "@/types/auth";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Select } from "@/components/Select";

const schema = z.object({
  email: z.email(),
  password: z.string().min(6, "Password must be at least 6 characters"),
  role: z.enum(["Customer", "ArtistManager", "VenueManager"]),
});

type FormData = z.infer<typeof schema>;

const roles: Exclude<Role, "Admin">[] = ["Customer", "ArtistManager", "VenueManager"];

export default function RegisterPage() {
  const register_ = useAuthStore((s) => s.register);
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { role: "Customer" },
  });

  const onSubmit = async (data: FormData) => {
    await register_(data);
    await navigate({ to: "/login" });
  };

  return (
    <form onSubmit={(e) => { void handleSubmit(onSubmit)(e); }} className="space-y-4">
      <h1>Register</h1>

      <div className="space-y-1">
        <Label htmlFor="email">Email</Label>
         
        {errors.email && <p>{errors.email.message}</p>}
      </div>

      <div className="space-y-1">
        <Label htmlFor="password">Password</Label>
        <Input id="password" type="password" {...register("password")} />
        {errors.password && <p>{errors.password.message}</p>}
      </div>

      <div className="space-y-1">
        <Label>I am a:</Label>
        <Select
          options={roles}
          onChange={(val: Exclude<Role, "Admin">) => { setValue("role", val); }}
          getLabel={(r) => r.replace(/([A-Z])/g, " $1").trim()}
          getValue={(r) => r}
        />
        {errors.role && <p>{errors.role.message}</p>}
      </div>

      <Button type="submit" disabled={isSubmitting}>
        {isSubmitting ? "Registering..." : "Register"}
      </Button>

      <p>
        Already have an account? <Link to="/login">Login</Link>
      </p>
    </form>
  );
}
