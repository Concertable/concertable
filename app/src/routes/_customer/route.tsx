import { createFileRoute, Outlet } from "@tanstack/react-router";
import { useCustomerNotifications } from "@/hooks/useNotifications";

function CustomerLayout() {
  useCustomerNotifications();
  return <Outlet />;
}

export const Route = createFileRoute("/_customer")({
  component: CustomerLayout,
});
