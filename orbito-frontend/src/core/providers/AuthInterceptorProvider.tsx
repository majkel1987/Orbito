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

  // Debug logging
  useEffect(() => {
    console.log("=== AUTH INTERCEPTOR DEBUG ===");
    console.log("Session status:", status);
    console.log("Session data:", session);
    console.log("Access token:", session?.accessToken);
    console.log("==============================");
  }, [session, status]);

  useEffect(() => {
    const interceptorId = axiosInstance.interceptors.request.use(
      (config) => {
        console.log("=== REQUEST INTERCEPTOR ===");
        console.log("Session accessToken:", session?.accessToken);
        console.log("Request URL:", config.url);

        if (session?.accessToken) {
          config.headers.Authorization = `Bearer ${session.accessToken}`;
          console.log("✅ Authorization header added");
        } else {
          console.log("❌ No accessToken in session!");
        }
        console.log("==========================");

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
