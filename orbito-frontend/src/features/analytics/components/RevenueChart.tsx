"use client";

import {
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from "recharts";
import { AlertCircle } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { Skeleton } from "@/shared/ui/skeleton";
import { formatCurrency } from "@/shared/lib/formatters";
import type { GetRevenueHistoryResponse } from "@/core/api/generated/models";

interface RevenueChartProps {
  data: GetRevenueHistoryResponse | undefined;
  isLoading: boolean;
  error: Error | null;
}

interface ChartDataPoint {
  date: string;
  amount: number;
  formattedDate: string;
}

function formatDate(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleDateString("pl-PL", { month: "short", day: "numeric" });
}

export function RevenueChart({ data, isLoading, error }: RevenueChartProps) {
  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Revenue Over Time</CardTitle>
        </CardHeader>
        <CardContent>
          <Skeleton className="h-[300px] w-full" />
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Revenue Over Time</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-red-500 p-4 flex items-center gap-2">
            <AlertCircle className="h-5 w-5" />
            Error loading revenue data: {error.message}
          </div>
        </CardContent>
      </Card>
    );
  }

  if (!data?.items?.length) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Revenue Over Time</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-muted-foreground p-4 h-[300px] flex items-center justify-center">
            No revenue data available for this period
          </div>
        </CardContent>
      </Card>
    );
  }

  const chartData: ChartDataPoint[] = data.items.map((item) => ({
    date: item.date || "",
    amount: item.amount || 0,
    formattedDate: formatDate(item.date || ""),
  }));

  const currency = data.items[0]?.currency || "PLN";

  return (
    <Card>
      <CardHeader>
        <CardTitle>Revenue Over Time</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="h-[300px]">
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={chartData}>
              <defs>
                <linearGradient id="colorRevenue" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.3} />
                  <stop offset="95%" stopColor="#3b82f6" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
              <XAxis
                dataKey="formattedDate"
                tick={{ fontSize: 12 }}
                tickLine={false}
                axisLine={false}
              />
              <YAxis
                tick={{ fontSize: 12 }}
                tickLine={false}
                axisLine={false}
                tickFormatter={(value) =>
                  formatCurrency(value, currency).replace(/,00\s*zł$/, " zł")
                }
              />
              <Tooltip
                content={({ active, payload }) => {
                  if (active && payload && payload.length) {
                    const data = payload[0].payload as ChartDataPoint;
                    return (
                      <div className="bg-background border rounded-lg shadow-lg p-3">
                        <p className="text-sm font-medium">{data.formattedDate}</p>
                        <p className="text-sm text-muted-foreground">
                          Revenue: {formatCurrency(data.amount, currency)}
                        </p>
                      </div>
                    );
                  }
                  return null;
                }}
              />
              <Area
                type="monotone"
                dataKey="amount"
                stroke="#3b82f6"
                strokeWidth={2}
                fillOpacity={1}
                fill="url(#colorRevenue)"
              />
            </AreaChart>
          </ResponsiveContainer>
        </div>
        <div className="mt-4 text-center text-sm text-muted-foreground">
          Total Revenue: {formatCurrency(data.totalRevenue || 0, currency)}
        </div>
      </CardContent>
    </Card>
  );
}
