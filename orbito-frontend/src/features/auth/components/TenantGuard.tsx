"use client";

import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

interface TenantGuardProps {
  children: React.ReactNode;
  requiredTenantId?: string;
}

export function TenantGuard({ children, requiredTenantId }: TenantGuardProps) {
  const { data: session, status } = useSession();
  const router = useRouter();

  useEffect(() => {
    if (status === "loading") return;

    // Brak sesji - middleware już przekierował, ale na wszelki wypadek
    if (!session?.user) {
      router.push("/login");
      return;
    }

    // Weryfikacja tenant context jeśli wymagany
    if (requiredTenantId && session.user.tenantId !== requiredTenantId) {
      // Użytkownik próbuje uzyskać dostęp do danych innego tenanta
      router.push("/unauthorized");
      return;
    }

    // Weryfikacja czy user ma tenantId
    if (!session.user.tenantId) {
      // User bez tenanta nie może uzyskać dostępu do chronionych zasobów
      router.push("/unauthorized");
      return;
    }
  }, [session, status, requiredTenantId, router]);

  // Loading state
  if (status === "loading") {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="text-muted-foreground">Ładowanie...</div>
      </div>
    );
  }

  // Brak sesji
  if (!session?.user) {
    return null;
  }

  // Nieprawidłowy tenant context
  if (requiredTenantId && session.user.tenantId !== requiredTenantId) {
    return null;
  }

  // Brak tenantId
  if (!session.user.tenantId) {
    return null;
  }

  return <>{children}</>;
}
