import { Outlet, Link } from "@tanstack/react-router";
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
  { label: "Settings", to: "/settings" },
  { label: "Payment / Billing", to: "/settings/payment" },
];

interface SettingsLayoutProps {
  extraLinks?: { label: string; to: string }[];
}

export function SettingsLayout({ extraLinks = [] }: SettingsLayoutProps) {
  const links = [...baseLinks, ...extraLinks];
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
