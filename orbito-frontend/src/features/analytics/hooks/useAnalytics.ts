"use client";

import {
  useGetApiAnalyticsDashboard,
  useGetApiAnalyticsRevenue,
  useGetApiAnalyticsClients,
} from "@/core/api/generated/analytics/analytics";
import type {
  DashboardStatsDto,
  GetRevenueHistoryResponse,
  GetClientGrowthResponse,
} from "@/core/api/generated/models";

export interface DateRange {
  from: Date;
  to: Date;
}

export function useAnalytics(dateRange: DateRange) {
  const dashboardQuery = useGetApiAnalyticsDashboard({
    startDate: dateRange.from.toISOString(),
    endDate: dateRange.to.toISOString(),
  });

  const revenueQuery = useGetApiAnalyticsRevenue({
    startDate: dateRange.from.toISOString(),
    endDate: dateRange.to.toISOString(),
  });

  const clientGrowthQuery = useGetApiAnalyticsClients({
    startDate: dateRange.from.toISOString(),
    endDate: dateRange.to.toISOString(),
  });

  return {
    // Dashboard stats
    stats: dashboardQuery.data as DashboardStatsDto | undefined,
    statsLoading: dashboardQuery.isLoading,
    statsError: dashboardQuery.error,

    // Revenue history
    revenueHistory: revenueQuery.data as GetRevenueHistoryResponse | undefined,
    revenueLoading: revenueQuery.isLoading,
    revenueError: revenueQuery.error,

    // Client growth
    clientGrowth: clientGrowthQuery.data as GetClientGrowthResponse | undefined,
    clientGrowthLoading: clientGrowthQuery.isLoading,
    clientGrowthError: clientGrowthQuery.error,

    // Combined loading state
    isLoading:
      dashboardQuery.isLoading ||
      revenueQuery.isLoading ||
      clientGrowthQuery.isLoading,

    // Refetch all
    refetchAll: () => {
      dashboardQuery.refetch();
      revenueQuery.refetch();
      clientGrowthQuery.refetch();
    },
  };
}

// Preset date ranges
export function getPresetDateRange(
  preset: "7d" | "30d" | "thisMonth" | "lastMonth" | "thisYear"
): DateRange {
  const now = new Date();
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());

  switch (preset) {
    case "7d":
      return {
        from: new Date(today.getTime() - 7 * 24 * 60 * 60 * 1000),
        to: today,
      };
    case "30d":
      return {
        from: new Date(today.getTime() - 30 * 24 * 60 * 60 * 1000),
        to: today,
      };
    case "thisMonth":
      return {
        from: new Date(now.getFullYear(), now.getMonth(), 1),
        to: today,
      };
    case "lastMonth":
      return {
        from: new Date(now.getFullYear(), now.getMonth() - 1, 1),
        to: new Date(now.getFullYear(), now.getMonth(), 0),
      };
    case "thisYear":
      return {
        from: new Date(now.getFullYear(), 0, 1),
        to: today,
      };
  }
}
