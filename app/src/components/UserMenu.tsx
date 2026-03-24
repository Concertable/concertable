import { useRouter } from "@tanstack/react-router";
import { UserCircle } from "lucide-react";
import { useAuthStore } from "@/store/useAuthStore";
import { Button } from "@/components/ui/button";
import { IconButton } from "@/components/IconButton";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuSub,
  DropdownMenuSubContent,
  DropdownMenuSubTrigger,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Link } from "@tanstack/react-router";

export function UserMenu() {
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);
  const router = useRouter();

  async function handleLogout() {
    await logout();
    router.navigate({ to: "/login" });
  }

  if (!user) {
    return (
      <div className="flex items-center gap-2">
        <Button variant="ghost" asChild>
          <Link to="/login">Login</Link>
        </Button>
        <Button asChild>
          <Link to="/register">Sign Up</Link>
        </Button>
      </div>
    );
  }

  const isCustomer = user.role === "Customer";

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <IconButton>
          <UserCircle className="size-7" />
        </IconButton>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-48">
        <DropdownMenuItem asChild>
          <Link to="/profile">Profile Details</Link>
        </DropdownMenuItem>

        {isCustomer && (
          <DropdownMenuItem asChild>
            <Link to="/profile/preferences">Preferences</Link>
          </DropdownMenuItem>
        )}

        {!isCustomer && (
          <DropdownMenuItem asChild>
            <Link to="/profile/payment">Payment</Link>
          </DropdownMenuItem>
        )}

        {isCustomer && (
          <DropdownMenuSub>
            <DropdownMenuSubTrigger>Tickets</DropdownMenuSubTrigger>
            <DropdownMenuSubContent>
              <DropdownMenuItem asChild>
                <Link to="/profile/tickets/upcoming">Upcoming</Link>
              </DropdownMenuItem>
              <DropdownMenuItem asChild>
                <Link to="/profile/tickets/history">History</Link>
              </DropdownMenuItem>
            </DropdownMenuSubContent>
          </DropdownMenuSub>
        )}

        <DropdownMenuSeparator />
        <DropdownMenuItem onClick={handleLogout} className="text-destructive">
          Logout
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
