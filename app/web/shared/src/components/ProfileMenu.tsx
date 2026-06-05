import { Link } from "@tanstack/react-router";
import { UserCircle } from "lucide-react";
import { useAuth } from "react-oidc-context";
import { useAuthStore } from "@/features/auth";
import { Button } from "@/components/ui/button";
import { IconButton } from "@/components/IconButton";
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

export interface ProfileMenuItem {
  label: string;
  to?: string;
  href?: string;
  items?: ProfileMenuItem[];
}

interface Props {
  items: ProfileMenuItem[];
}

export function ProfileMenu({ items }: Readonly<Props>) {
  const user = useAuthStore((s) => s.user);
  const auth = useAuth();

  async function handleLogout() {
    await auth.signoutRedirect();
  }

  if (!user) {
    return (
      <div className="flex items-center gap-2">
        <Button variant="ghost" asChild>
          <Link to="/login" data-testid="header-login">Login</Link>
        </Button>
        <Button asChild>
          <Link to="/register" data-testid="header-register">Sign Up</Link>
        </Button>
      </div>
    );
  }

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

        {items.map((item) => (
          <MenuItem key={item.label} item={item} />
        ))}

        <DropdownMenuItem asChild>
          <Link to="/settings">Settings</Link>
        </DropdownMenuItem>
        <DropdownMenuItem asChild>
          <Link to="/settings/payment">Payment / Billing</Link>
        </DropdownMenuItem>

        <DropdownMenuSeparator />
        <DropdownMenuItem onClick={handleLogout} className="text-destructive">
          Logout
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

function MenuItem({ item }: Readonly<{ item: ProfileMenuItem }>) {
  if (item.items)
    return (
      <DropdownMenuSub>
        <DropdownMenuSubTrigger>{item.label}</DropdownMenuSubTrigger>
        <DropdownMenuSubContent>
          {item.items.map((child) => (
            <MenuItem key={child.label} item={child} />
          ))}
        </DropdownMenuSubContent>
      </DropdownMenuSub>
    );

  if (item.href)
    return (
      <DropdownMenuItem asChild>
        <a href={item.href} target="_blank" rel="noopener noreferrer">
          {item.label}
        </a>
      </DropdownMenuItem>
    );

  return (
    <DropdownMenuItem asChild>
      <Link to={item.to!}>{item.label}</Link>
    </DropdownMenuItem>
  );
}
