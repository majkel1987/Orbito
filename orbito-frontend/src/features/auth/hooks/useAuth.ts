"use client";

import { useSession } from "next-auth/react";

export function useAuth() {
  const { data: session, status } = useSession();

  return {
    user: session?.user,
    isLoading: status === "loading",
    isAuthenticated: status === "authenticated",
    role: session?.user?.role,
    tenantId: session?.user?.tenantId,
    userId: session?.user?.id,
    accessToken: session?.accessToken,
  };
}
