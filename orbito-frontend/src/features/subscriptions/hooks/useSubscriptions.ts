"use client";

import { useSearchParams, useRouter } from "next/navigation";
import { useGetApiSubscriptions } from "@/core/api/generated/subscriptions/subscriptions";
import type { SubscriptionDtoPaginatedList } from "../types/subscription.types";

/**
 * Hook for managing subscriptions list with pagination and search
 * Uses URL search params for state persistence
 */
export function useSubscriptions() {
  const router = useRouter();
  const searchParams = useSearchParams();

  // Read params from URL
  const currentPage = Number(searchParams.get("page")) || 1;
  const searchTerm = searchParams.get("search") || "";

  // Fetch subscriptions with type assertion (Orval bug: returns customInstance<void>)
  const { data, isLoading, error } = useGetApiSubscriptions({
    pageNumber: currentPage,
    pageSize: 50,
    searchTerm: searchTerm || undefined,
  }) as {
    data: SubscriptionDtoPaginatedList | undefined;
    isLoading: boolean;
    error: Error | null;
  };

  // Handle pagination
  const goToPage = (page: number) => {
    const params = new URLSearchParams(searchParams.toString());
    params.set("page", page.toString());
    router.push(`?${params.toString()}`);
  };

  // Handle search
  const setSearch = (search: string) => {
    const params = new URLSearchParams(searchParams.toString());
    if (search) {
      params.set("search", search);
    } else {
      params.delete("search");
    }
    params.set("page", "1"); // Reset to first page when searching
    router.push(`?${params.toString()}`);
  };

  // Clear all filters
  const clearFilters = () => {
    router.push("/dashboard/subscriptions");
  };

  return {
    subscriptions: data?.items || [],
    pagination: data
      ? {
          currentPage: data.pageNumber,
          pageSize: data.pageSize,
          totalCount: data.totalCount,
          totalPages: data.totalPages,
          hasPreviousPage: data.hasPreviousPage,
          hasNextPage: data.hasNextPage,
        }
      : null,
    isLoading,
    error,
    searchTerm,
    goToPage,
    setSearch,
    clearFilters,
    hasActiveFilters: searchTerm.length > 0,
  };
}
