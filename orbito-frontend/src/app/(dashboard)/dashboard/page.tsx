"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { useGetApiClients } from "@/core/api/generated/clients/clients";
import { useGetApiSubscriptions } from "@/core/api/generated/subscriptions/subscriptions";
import { useGetApiPaymentMetricsStatistics } from "@/core/api/generated/payment-metrics/payment-metrics";
import { formatCurrency } from "@/shared/lib/formatters";
import { Skeleton } from "@/shared/ui/skeleton";
import type {
  ClientDtoPaginatedList,
  PaymentStatistics,
} from "@/core/api/generated/models";

export default function DashboardPage() {
  // Fetch clients count
  const { data: clientsData, isLoading: isLoadingClients } = useGetApiClients({
    pageNumber: 1,
    pageSize: 1,
  }) as { data: ClientDtoPaginatedList | undefined; isLoading: boolean };

  // Fetch subscriptions
  const { data: subscriptionsData, isLoading: isLoadingSubscriptions } =
    useGetApiSubscriptions() as {
      data:
        | { items: Array<{ status: string }>; totalCount: number }
        | undefined;
      isLoading: boolean;
    };

  // Fetch payment statistics for current month
  const now = new Date();
  const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);
  const endOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0);

  const {
    data: statsData,
    isLoading: isLoadingStats,
    error: statsError,
  } = useGetApiPaymentMetricsStatistics({
    startDate: startOfMonth.toISOString(),
    endDate: endOfMonth.toISOString(),
  }) as {
    data: PaymentStatistics | undefined;
    isLoading: boolean;
    error: Error | null;
  };

  const totalClients = clientsData?.totalCount ?? 0;
  const activeSubscriptions =
    subscriptionsData?.items?.filter((s) => s.status === "Active").length ?? 0;
  // PaymentMetrics might not be available if service is not implemented
  const monthlyRevenue = !statsError ? statsData?.totalRevenue ?? 0 : null;
  const currency = statsData?.currency ?? "PLN";

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
        <p className="text-muted-foreground">
          Welcome to your Orbito dashboard
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle>Total Clients</CardTitle>
          </CardHeader>
          <CardContent>
            {isLoadingClients ? (
              <Skeleton className="h-10 w-20" />
            ) : (
              <p className="text-3xl font-bold">{totalClients}</p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Active Subscriptions</CardTitle>
          </CardHeader>
          <CardContent>
            {isLoadingSubscriptions ? (
              <Skeleton className="h-10 w-20" />
            ) : (
              <p className="text-3xl font-bold">{activeSubscriptions}</p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Monthly Revenue</CardTitle>
          </CardHeader>
          <CardContent>
            {isLoadingStats ? (
              <Skeleton className="h-10 w-32" />
            ) : statsError ? (
              <p className="text-sm text-muted-foreground">
                Stats temporarily unavailable
              </p>
            ) : (
              <p className="text-3xl font-bold">
                {formatCurrency(monthlyRevenue ?? 0, currency)}
              </p>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
