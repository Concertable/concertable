import { useRouter, Link } from "@tanstack/react-router";
import { UserCircle } from "lucide-react";
import { useAuthStore } from "@/store/useAuthStore";
import { useNavSection } from "@/hooks/useNavSection";
import { Button } from "@/components/ui/button";
import { IconButton } from "@/components/IconButton";
import { isArtistManager, isVenueManager } from "@/types/auth";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuSub,
  DropdownMenuSubContent,
  DropdownMenuSubTrigger,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

export function ProfileMenu() {
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);
  const router = useRouter();
  const section = useNavSection();

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
  const isAdmin = user.role === "Admin";
  const inCustomerView = section === "Customer";

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <IconButton>
          <UserCircle className="size-7" />
        </IconButton>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-52">
        <DropdownMenuLabel className="text-muted-foreground truncate text-xs font-normal">
          {user.email}
        </DropdownMenuLabel>

        <DropdownMenuSeparator />

        {isCustomer && (
          <DropdownMenuItem asChild>
            <Link to="/profile">Profile</Link>
          </DropdownMenuItem>
        )}
        <DropdownMenuItem asChild>
          <Link to="/settings">Settings</Link>
        </DropdownMenuItem>
        <DropdownMenuItem asChild>
          <Link to="/settings/payment">Payment / Billing</Link>
        </DropdownMenuItem>

        {isCustomer && (
          <>
            <DropdownMenuSub>
              <DropdownMenuSubTrigger>My Tickets</DropdownMenuSubTrigger>
              <DropdownMenuSubContent>
                <DropdownMenuItem asChild>
                  <Link to="/profile/tickets/upcoming">Upcoming</Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                  <Link to="/profile/tickets/history">History</Link>
                </DropdownMenuItem>
              </DropdownMenuSubContent>
            </DropdownMenuSub>
            <DropdownMenuItem asChild>
              <Link to="/profile/preferences">Preferences</Link>
            </DropdownMenuItem>
          </>
        )}

        {isArtistManager(user) && (
          <>
            <DropdownMenuItem asChild>
              <Link to="/artist/my">My Artist</Link>
            </DropdownMenuItem>
            <DropdownMenuItem asChild>
              <Link to="/artist">Dashboard</Link>
            </DropdownMenuItem>
          </>
        )}

        {isVenueManager(user) && (
          <>
            <DropdownMenuItem asChild>
              <Link to="/venue/my">My Venue</Link>
            </DropdownMenuItem>
            <DropdownMenuItem asChild>
              <Link to="/venue">Dashboard</Link>
            </DropdownMenuItem>
          </>
        )}

        {!isCustomer && !isAdmin && (
          <>
            <DropdownMenuSeparator />
            {inCustomerView ? (
              <DropdownMenuItem asChild>
                <Link to={user.baseUrl}>Manager View</Link>
              </DropdownMenuItem>
            ) : (
              <DropdownMenuItem asChild>
                <Link to="/">Customer View</Link>
              </DropdownMenuItem>
            )}
          </>
        )}

        <DropdownMenuSeparator />
        <DropdownMenuItem onClick={handleLogout} className="text-destructive">
          Logout
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
