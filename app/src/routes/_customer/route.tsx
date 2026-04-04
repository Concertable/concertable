import { createFileRoute } from "@tanstack/react-router";
import { useCustomerNotifications } from "@/hooks/useNotifications";
import { AppLayout } from "@/components/AppLayout";

const links = [
  { label: "Home", to: "/" },
  { label: "Find", to: "/find" },
  { label: "Settings", to: "/settings" },
];

function CustomerLayout() {
  useCustomerNotifications();
  return <AppLayout links={links} />;
}

export const Route = createFileRoute("/_customer")({
  component: CustomerLayout,
});
