"use client";

import { SessionProvider } from "next-auth/react";
import { AuthInterceptorProvider } from "@/core/providers/AuthInterceptorProvider";
import { PortalGuard } from "@/features/client-portal/components/PortalGuard";
import { UserMenu } from "@/shared/components/layout/UserMenu";

export default function PortalLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <SessionProvider>
      <AuthInterceptorProvider>
        <PortalGuard>
          <div className="min-h-screen bg-background">
            {/* Simple top navbar - no sidebar */}
            <header className="sticky top-0 z-30 border-b bg-background">
              <div className="mx-auto flex h-16 max-w-4xl items-center justify-between px-4 sm:px-6">
                <div className="flex items-center gap-2">
                  <div className="flex h-8 w-8 items-center justify-center rounded-md bg-primary text-primary-foreground">
                    <span className="text-lg font-bold">O</span>
                  </div>
                  <span className="text-xl font-bold">Orbito</span>
                  <span className="text-sm text-muted-foreground">/ Portal</span>
                </div>
                <UserMenu />
              </div>
            </header>

            {/* Main content - centered max-w-4xl */}
            <main className="mx-auto max-w-4xl px-4 py-8 sm:px-6">
              {children}
            </main>
          </div>
        </PortalGuard>
      </AuthInterceptorProvider>
    </SessionProvider>
  );
}
