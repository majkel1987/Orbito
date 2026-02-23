"use client";

import {
  DollarSign,
  TrendingUp,
  Users,
  CreditCard,
  AlertCircle,
} from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { Skeleton } from "@/shared/ui/skeleton";
import { formatCurrency } from "@/shared/lib/formatters";
import type { DashboardStatsDto } from "@/core/api/generated/models";

interface StatCardsProps {
  stats: DashboardStatsDto | undefined;
  isLoading: boolean;
  error: Error | null;
}

interface StatCardProps {
  title: string;
  value: string | number;
  icon: React.ReactNode;
  description?: string;
  trend?: string;
}

function StatCard({ title, value, icon, description, trend }: StatCardProps) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-sm font-medium">{title}</CardTitle>
        {icon}
      </CardHeader>
      <CardContent>
        <div className="text-2xl font-bold">{value}</div>
        {description && (
          <p className="text-xs text-muted-foreground">{description}</p>
        )}
        {trend && (
          <p className="text-xs text-green-600 flex items-center gap-1 mt-1">
            <TrendingUp className="h-3 w-3" />
            {trend}
          </p>
        )}
      </CardContent>
    </Card>
  );
}

function StatCardSkeleton() {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <Skeleton className="h-4 w-[100px]" />
        <Skeleton className="h-4 w-4" />
      </CardHeader>
      <CardContent>
        <Skeleton className="h-8 w-[120px]" />
        <Skeleton className="h-3 w-[80px] mt-2" />
      </CardContent>
    </Card>
  );
}

export function StatCards({ stats, isLoading, error }: StatCardsProps) {
  if (isLoading) {
    return (
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <StatCardSkeleton />
        <StatCardSkeleton />
        <StatCardSkeleton />
        <StatCardSkeleton />
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-red-500 p-4 border border-red-200 rounded-lg flex items-center gap-2">
        <AlertCircle className="h-5 w-5" />
        Error loading statistics: {error.message}
      </div>
    );
  }

  if (!stats) {
    return (
      <div className="text-muted-foreground p-4 border rounded-lg">
        No statistics available
      </div>
    );
  }

  const currency = stats.currency || "PLN";

  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
      <StatCard
        title="Monthly Recurring Revenue"
        value={formatCurrency(stats.mrr || 0, currency)}
        icon={<DollarSign className="h-4 w-4 text-muted-foreground" />}
        description="MRR from active subscriptions"
      />
      <StatCard
        title="Annual Recurring Revenue"
        value={formatCurrency(stats.arr || 0, currency)}
        icon={<TrendingUp className="h-4 w-4 text-muted-foreground" />}
        description="MRR x 12"
      />
      <StatCard
        title="Active Clients"
        value={stats.totalClients || 0}
        icon={<Users className="h-4 w-4 text-muted-foreground" />}
        description={`${stats.newClients || 0} new this period`}
      />
      <StatCard
        title="Active Subscriptions"
        value={stats.activeSubscriptions || 0}
        icon={<CreditCard className="h-4 w-4 text-muted-foreground" />}
        description={`Churn rate: ${(stats.churnRate || 0).toFixed(1)}%`}
      />
    </div>
  );
}
