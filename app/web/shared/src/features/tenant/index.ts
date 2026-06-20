export { useActiveTenantStore } from "./store/useActiveTenantStore";

/** Wire header naming the acting tenant; mirrors the backend's TenantHeaders.TenantId. */
export const TENANT_HEADER = "X-Tenant-Id";
