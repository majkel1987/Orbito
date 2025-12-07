import { useTenantStore } from "../store/tenant.store";

export function useTenant() {
  const { tenantId, tenantName, setTenant, clearTenant } = useTenantStore();

  return {
    tenantId,
    tenantName,
    setTenant,
    clearTenant,
    isReady: !!tenantId,
  };
}
