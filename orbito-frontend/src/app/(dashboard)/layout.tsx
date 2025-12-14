"use client";

import { SessionProvider } from "next-auth/react";
import { TenantProvider } from "@/features/auth";
import { DashboardShell } from "@/shared/components/layout/DashboardShell";
import { AuthInterceptorProvider } from "@/core/providers/AuthInterceptorProvider";

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <SessionProvider>
      <AuthInterceptorProvider>
        <TenantProvider>
          <DashboardShell>{children}</DashboardShell>
        </TenantProvider>
      </AuthInterceptorProvider>
    </SessionProvider>
  );
}
