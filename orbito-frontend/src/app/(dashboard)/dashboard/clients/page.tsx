"use client";

import { useGetApiClients } from "@/core/api/generated/clients/clients";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { Badge } from "@/shared/ui/badge";
import { Skeleton } from "@/shared/ui/skeleton";
import { formatDate } from "@/shared/lib/formatters";
import type { ClientDtoPaginatedList } from "@/core/api/generated/models";

export default function ClientsPage() {
  const { data, isLoading, error } = useGetApiClients({
    pageNumber: 1,
    pageSize: 50,
  }) as {
    data: ClientDtoPaginatedList | undefined;
    isLoading: boolean;
    error: Error | null;
  };

  if (error) {
    return (
      <div className="space-y-4">
        <h1 className="text-3xl font-bold tracking-tight">Clients</h1>
        <Card>
          <CardContent className="pt-6">
            <p className="text-destructive">
              Failed to load clients. {error.message}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Clients</h1>
          <p className="text-muted-foreground">
            Manage your clients ({data?.totalCount ?? 0} total)
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>All Clients</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="space-y-2">
              <Skeleton className="h-12 w-full" />
              <Skeleton className="h-12 w-full" />
              <Skeleton className="h-12 w-full" />
            </div>
          ) : data?.items && data.items.length > 0 ? (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b">
                    <th className="text-left p-2 font-medium">Name</th>
                    <th className="text-left p-2 font-medium">Email</th>
                    <th className="text-left p-2 font-medium">Company</th>
                    <th className="text-left p-2 font-medium">Status</th>
                    <th className="text-left p-2 font-medium">Created</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((client) => (
                    <tr key={client.id} className="border-b hover:bg-muted/50">
                      <td className="p-2">
                        {client.fullName || client.directFirstName
                          ? `${client.directFirstName || ""} ${client.directLastName || ""}`.trim()
                          : "—"}
                      </td>
                      <td className="p-2">
                        {client.email || client.directEmail || "—"}
                      </td>
                      <td className="p-2">{client.companyName || "—"}</td>
                      <td className="p-2">
                        {client.isActive ? (
                          <Badge variant="default">Active</Badge>
                        ) : (
                          <Badge variant="destructive">Inactive</Badge>
                        )}
                      </td>
                      <td className="p-2">
                        {client.createdAt
                          ? formatDate(client.createdAt)
                          : "—"}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <div className="text-center py-8 text-muted-foreground">
              No clients found. Create your first client to get started.
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
