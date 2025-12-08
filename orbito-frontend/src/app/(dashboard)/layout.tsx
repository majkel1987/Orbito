"use client";

import { useState } from "react";
import { SessionProvider } from "next-auth/react";
import { TenantProvider } from "@/features/auth";
import { Sidebar } from "@/shared/components/layout/Sidebar";
import { Header } from "@/shared/components/layout/Header";

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  return (
    <SessionProvider>
      <TenantProvider>
        <div className="relative min-h-screen">
          {/* Sidebar - hidden on mobile, fixed on desktop */}
          <div
            className={`
              fixed inset-y-0 left-0 z-50 md:relative md:block
              ${sidebarOpen ? "block" : "hidden"}
            `}
          >
            <Sidebar />
          </div>

          {/* Mobile overlay */}
          {sidebarOpen && (
            <div
              className="fixed inset-0 z-40 bg-black/50 md:hidden"
              onClick={() => setSidebarOpen(false)}
            />
          )}

          {/* Main content area */}
          <div className="flex min-h-screen flex-col md:pl-64">
            <Header onMenuClick={() => setSidebarOpen(!sidebarOpen)} />
            <main className="flex-1 p-4 sm:p-6">{children}</main>
          </div>
        </div>
      </TenantProvider>
    </SessionProvider>
  );
}
