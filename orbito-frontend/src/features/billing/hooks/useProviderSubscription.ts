"use client";

import { usePathname } from "next/navigation";
import { useGetApiProviderSubscriptionMy } from "@/core/api/generated/provider-subscription/provider-subscription";

/**
 * Hook for getting provider's platform subscription status.
 * Used for displaying trial banner and subscription expired overlay in the dashboard.
 */
export function useProviderSubscription() {
  const { data, isLoading, error, refetch } = useGetApiProviderSubscriptionMy();
  const pathname = usePathname();

  const isExpired = data?.status === "Expired" || data?.isExpired === true;
  const isBillingPage = pathname?.startsWith("/dashboard/billing") ?? false;

  // Provider should be blocked from accessing dashboard features if:
  // - Their subscription is expired AND
  // - They are NOT on the billing page (they need access to pay!)
  const shouldBlock = isExpired && !isBillingPage;

  return {
    subscription: data ?? null,
    isLoading,
    error,
    refetch,
    isTrial: data?.status === "Trial",
    isActive: data?.status === "Active",
    isExpired,
    isBillingPage,
    shouldBlock,
    daysRemaining: data?.daysRemaining ?? 0,
    planName: data?.planName ?? "",
    trialEndDate: data?.trialEndDate ? new Date(data.trialEndDate) : null,
  };
}
