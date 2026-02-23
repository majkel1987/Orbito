"use client";

import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from "recharts";
import { AlertCircle } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { Skeleton } from "@/shared/ui/skeleton";
import type { GetClientGrowthResponse } from "@/core/api/generated/models";

interface ClientGrowthChartProps {
  data: GetClientGrowthResponse | undefined;
  isLoading: boolean;
  error: Error | null;
}

interface ChartDataPoint {
  date: string;
  totalClients: number;
  newClients: number;
  formattedDate: string;
}

function formatDate(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleDateString("pl-PL", { month: "short", day: "numeric" });
}

export function ClientGrowthChart({
  data,
  isLoading,
  error,
}: ClientGrowthChartProps) {
  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Client Growth</CardTitle>
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
          <CardTitle>Client Growth</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-red-500 p-4 flex items-center gap-2">
            <AlertCircle className="h-5 w-5" />
            Error loading client data: {error.message}
          </div>
        </CardContent>
      </Card>
    );
  }

  if (!data?.items?.length) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Client Growth</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-muted-foreground p-4 h-[300px] flex items-center justify-center">
            No client data available for this period
          </div>
        </CardContent>
      </Card>
    );
  }

  const chartData: ChartDataPoint[] = data.items.map((item) => ({
    date: item.date || "",
    totalClients: item.totalClients || 0,
    newClients: item.newClients || 0,
    formattedDate: formatDate(item.date || ""),
  }));

  return (
    <Card>
      <CardHeader>
        <CardTitle>Client Growth</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="h-[300px]">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={chartData}>
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
              />
              <Tooltip
                content={({ active, payload }) => {
                  if (active && payload && payload.length) {
                    const data = payload[0].payload as ChartDataPoint;
                    return (
                      <div className="bg-background border rounded-lg shadow-lg p-3">
                        <p className="text-sm font-medium">{data.formattedDate}</p>
                        <p className="text-sm text-muted-foreground">
                          New Clients: {data.newClients}
                        </p>
                        <p className="text-sm text-muted-foreground">
                          Total Clients: {data.totalClients}
                        </p>
                      </div>
                    );
                  }
                  return null;
                }}
              />
              <Bar
                dataKey="newClients"
                fill="#10b981"
                radius={[4, 4, 0, 0]}
                name="New Clients"
              />
            </BarChart>
          </ResponsiveContainer>
        </div>
        <div className="mt-4 flex justify-center gap-8 text-sm text-muted-foreground">
          <span>Total Clients: {data.totalClients || 0}</span>
          <span>New in Period: {data.newClientsInPeriod || 0}</span>
        </div>
      </CardContent>
    </Card>
  );
}
