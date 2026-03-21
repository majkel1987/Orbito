"use client";

import { useGetApiProviderSubscriptionMy } from "@/core/api/generated/provider-subscription/provider-subscription";

/**
 * Hook for getting provider's platform subscription status.
 * Used for displaying trial banner in the dashboard.
 */
export function useProviderSubscription() {
  const { data, isLoading, error, refetch } = useGetApiProviderSubscriptionMy();

  return {
    subscription: data ?? null,
    isLoading,
    error,
    refetch,
    isTrial: data?.status === "Trial",
    isActive: data?.status === "Active",
    isExpired: data?.status === "Expired" || data?.isExpired === true,
    daysRemaining: data?.daysRemaining ?? 0,
    planName: data?.planName ?? "",
    trialEndDate: data?.trialEndDate ? new Date(data.trialEndDate) : null,
  };
}
