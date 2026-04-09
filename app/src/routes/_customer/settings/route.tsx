import { createFileRoute, Outlet, Link } from "@tanstack/react-router";
import { useAuthStore } from "@/store/useAuthStore";
import { requireAuth } from "@/lib/guards";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarInset,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
} from "@/components/ui/sidebar";

const baseLinks = [
  { label: "Account", to: "/settings" },
  { label: "Payment / Billing", to: "/settings/payment" },
];

const customerLinks = [
  { label: "Location", to: "/settings/location" },
  { label: "Preferences", to: "/settings/preferences" },
  { label: "My Tickets", to: "/settings/tickets" },
];

function SettingsLayout() {
  const user = useAuthStore((s) => s.user);
  const isCustomer = user?.role === "Customer";
  const links = isCustomer ? [...baseLinks, ...customerLinks] : baseLinks;

  return (
    <SidebarProvider
      style={{ "--sidebar-width": "220px" } as React.CSSProperties}
      className="min-h-screen"
    >
      <Sidebar collapsible="none" className="bg-muted min-h-screen">
        <SidebarContent>
          <SidebarGroup>
            <SidebarGroupLabel>Settings</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>
                {links.map(({ label, to }) => (
                  <SidebarMenuItem key={to}>
                    <SidebarMenuButton asChild>
                      <Link
                        to={to}
                        activeOptions={{ exact: to === "/settings" }}
                        activeProps={{
                          "data-active": true,
                          className:
                            "bg-primary! text-primary-foreground! hover:bg-primary! hover:text-primary-foreground!",
                        }}
                      >
                        {label}
                      </Link>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                ))}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        </SidebarContent>
      </Sidebar>

      <SidebarInset>
        <div className="p-6">
          <Outlet />
        </div>
      </SidebarInset>
    </SidebarProvider>
  );
}

export const Route = createFileRoute("/_customer/settings")({
  beforeLoad: ({ location }) => requireAuth({ location }),
  component: SettingsLayout,
});
