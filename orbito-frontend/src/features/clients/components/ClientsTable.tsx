"use client";

import { useClients } from "../hooks/useClients";
import { ClientsFilters } from "./ClientsFilters";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/ui/table";
import { Badge } from "@/shared/ui/badge";
import { Skeleton } from "@/shared/ui/skeleton";
import { Pagination } from "@/shared/components/Pagination";
import Link from "next/link";

export function ClientsTable() {
  const {
    clients,
    isLoading,
    error,
    currentPage,
    totalPages,
    search,
    status,
    setPage,
    setSearch,
    setStatus,
    clearFilters,
  } = useClients();

  if (error) {
    return (
      <div className="rounded-md bg-red-50 p-4 text-red-500">
        Error: {error.message}
      </div>
    );
  }

  if (isLoading) {
    return <ClientsTableSkeleton />;
  }

  if (clients.length === 0) {
    return (
      <div className="space-y-4">
        <ClientsFilters
          search={search}
          status={status}
          onSearchChange={setSearch}
          onStatusChange={setStatus}
          onClear={clearFilters}
        />
        <div className="text-center py-12 text-muted-foreground">
          <p className="text-lg font-semibold">No clients found</p>
          <p className="text-sm">Try adjusting your search or filters.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <ClientsFilters
        search={search}
        status={status}
        onSearchChange={setSearch}
        onStatusChange={setStatus}
        onClear={clearFilters}
      />

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Email</TableHead>
              <TableHead>Company</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {clients.map((client) => (
              <TableRow key={client.id}>
                <TableCell className="font-medium">
                  {client.fullName || "-"}
                </TableCell>
                <TableCell>{client.email || client.directEmail || "-"}</TableCell>
                <TableCell>{client.companyName || "-"}</TableCell>
                <TableCell>
                  <Badge variant={client.isActive ? "default" : "secondary"}>
                    {client.isActive ? "Active" : "Inactive"}
                  </Badge>
                </TableCell>
                <TableCell className="text-right">
                  <Link
                    href={`/dashboard/clients/${client.id}`}
                    className="text-sm text-primary hover:underline"
                  >
                    View
                  </Link>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {totalPages > 1 && (
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          onPageChange={setPage}
        />
      )}
    </div>
  );
}

function ClientsTableSkeleton() {
  return (
    <div className="space-y-2">
      {[...Array(5)].map((_, i) => (
        <Skeleton key={i} className="h-16 w-full" />
      ))}
    </div>
  );
}
