"use client";

import { useTenant } from "../hooks/useTenant";

export function TenantSwitcher() {
  const { tenantName, isReady } = useTenant();

  if (!isReady) {
    return (
      <div className="text-muted-foreground text-sm">Loading tenant...</div>
    );
  }

  return (
    <div className="flex items-center gap-2">
      <div className="flex flex-col">
        <span className="text-xs text-muted-foreground">Current Tenant</span>
        <span className="font-medium text-sm">{tenantName}</span>
      </div>
    </div>
  );
}
