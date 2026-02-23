"use client";

import { useState } from "react";
import { Download } from "lucide-react";
import { Button } from "@/shared/ui/button";
import {
  useAnalytics,
  getPresetDateRange,
  type DateRange,
} from "@/features/analytics/hooks/useAnalytics";
import { DateRangePicker } from "@/features/analytics/components/DateRangePicker";
import { StatCards } from "@/features/analytics/components/StatCards";
import { RevenueChart } from "@/features/analytics/components/RevenueChart";
import { ClientGrowthChart } from "@/features/analytics/components/ClientGrowthChart";
import { formatCurrency } from "@/shared/lib/formatters";

function exportToCSV(
  stats: ReturnType<typeof useAnalytics>["stats"],
  revenueHistory: ReturnType<typeof useAnalytics>["revenueHistory"],
  dateRange: DateRange
) {
  if (!stats && !revenueHistory) {
    alert("No data to export");
    return;
  }

  const lines: string[] = [];

  // Header
  lines.push("Orbito Analytics Report");
  lines.push(
    `Period: ${dateRange.from.toLocaleDateString()} - ${dateRange.to.toLocaleDateString()}`
  );
  lines.push("");

  // Stats
  if (stats) {
    lines.push("Key Metrics");
    lines.push(`MRR,${stats.mrr || 0}`);
    lines.push(`ARR,${stats.arr || 0}`);
    lines.push(`Total Clients,${stats.totalClients || 0}`);
    lines.push(`Active Subscriptions,${stats.activeSubscriptions || 0}`);
    lines.push(`Churn Rate,${stats.churnRate || 0}%`);
    lines.push(`Total Revenue,${stats.totalRevenue || 0}`);
    lines.push("");
  }

  // Revenue history
  if (revenueHistory?.items?.length) {
    lines.push("Revenue History");
    lines.push("Date,Amount,Currency");
    revenueHistory.items.forEach((item) => {
      lines.push(`${item.date},${item.amount},${item.currency}`);
    });
  }

  // Create and download file
  const csvContent = lines.join("\n");
  const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
  const link = document.createElement("a");
  link.href = URL.createObjectURL(blob);
  link.download = `analytics-report-${dateRange.from.toISOString().split("T")[0]}.csv`;
  link.click();
}

export default function AnalyticsPage() {
  const [dateRange, setDateRange] = useState<DateRange>(() =>
    getPresetDateRange("30d")
  );

  const {
    stats,
    statsLoading,
    statsError,
    revenueHistory,
    revenueLoading,
    revenueError,
    clientGrowth,
    clientGrowthLoading,
    clientGrowthError,
  } = useAnalytics(dateRange);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Analytics</h1>
          <p className="text-muted-foreground">
            Track your business performance and growth
          </p>
        </div>
        <div className="flex items-center gap-2">
          <DateRangePicker
            dateRange={dateRange}
            onDateRangeChange={setDateRange}
          />
          <Button
            variant="outline"
            onClick={() => exportToCSV(stats, revenueHistory, dateRange)}
          >
            <Download className="mr-2 h-4 w-4" />
            Export Report
          </Button>
        </div>
      </div>

      {/* Stat Cards */}
      <StatCards
        stats={stats}
        isLoading={statsLoading}
        error={statsError as Error | null}
      />

      {/* Charts */}
      <div className="grid gap-6 md:grid-cols-2">
        <RevenueChart
          data={revenueHistory}
          isLoading={revenueLoading}
          error={revenueError as Error | null}
        />
        <ClientGrowthChart
          data={clientGrowth}
          isLoading={clientGrowthLoading}
          error={clientGrowthError as Error | null}
        />
      </div>

      {/* Summary Footer */}
      {stats && (
        <div className="rounded-lg border bg-card p-6">
          <h3 className="text-lg font-semibold mb-4">Period Summary</h3>
          <div className="grid gap-4 md:grid-cols-3">
            <div>
              <p className="text-sm text-muted-foreground">Total Revenue</p>
              <p className="text-2xl font-bold">
                {formatCurrency(stats.totalRevenue || 0, stats.currency || "PLN")}
              </p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">New Clients</p>
              <p className="text-2xl font-bold">{stats.newClients || 0}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Cancelled Subscriptions</p>
              <p className="text-2xl font-bold">{stats.cancelledSubscriptions || 0}</p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
