"use client";

import { useSession } from "next-auth/react";
import { useEffect } from "react";
import { useTenantStore } from "../store/tenant.store";

interface TenantProviderProps {
  children: React.ReactNode;
}

export function TenantProvider({ children }: TenantProviderProps) {
  const { data: session, status } = useSession();
  const { setTenant, clearTenant } = useTenantStore();

  useEffect(() => {
    if (status === "loading") return;

    if (session?.user?.tenantId && session.user.name) {
      // User zalogowany i ma tenant context - ustaw w store
      setTenant(session.user.tenantId, session.user.name);
    } else {
      // Brak sesji lub brak tenant context - wyczyść store
      clearTenant();
    }
  }, [session, status, setTenant, clearTenant]);

  return <>{children}</>;
}
