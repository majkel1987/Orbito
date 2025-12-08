import { SessionProvider } from "next-auth/react";
import { TenantProvider } from "@/features/auth";
import { DashboardShell } from "@/shared/components/layout/DashboardShell";

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <SessionProvider>
      <TenantProvider>
        <DashboardShell>{children}</DashboardShell>
      </TenantProvider>
    </SessionProvider>
  );
}
