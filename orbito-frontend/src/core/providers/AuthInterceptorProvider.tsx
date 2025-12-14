"use client";

import { useSession } from "next-auth/react";
import { useEffect } from "react";
import axiosInstance from "@/core/api/client";

export function AuthInterceptorProvider({
  children,
}: {
  children: React.ReactNode;
}) {
  const { data: session, status } = useSession();

  useEffect(() => {
    const interceptorId = axiosInstance.interceptors.request.use(
      (config) => {
        if (session?.accessToken) {
          config.headers.Authorization = `Bearer ${session.accessToken}`;
        }
        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );

    // Cleanup function
    return () => {
      axiosInstance.interceptors.request.eject(interceptorId);
    };
  }, [session, status]);

  // Wait for session to be loaded before rendering children
  if (status === "loading") {
    return null;
  }

  return <>{children}</>;
}
