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
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/shared/ui/dropdown-menu";
import { Button } from "@/shared/ui/button";
import { MoreHorizontal, Eye, Send } from "lucide-react";
import Link from "next/link";
import { usePostApiClientsIdResendInvitation } from "@/core/api/generated/clients/clients";
import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

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

  const queryClient = useQueryClient();

  const resendMutation = usePostApiClientsIdResendInvitation({
    mutation: {
      onSuccess: () => {
        toast.success("Zaproszenie zostało ponownie wysłane!");
        queryClient.invalidateQueries({ queryKey: ["/api/Clients"] });
      },
      onError: (error: unknown) => {
        const message =
          error instanceof Error ? error.message : "Nie udało się wysłać zaproszenia";
        toast.error(message);
      },
    },
  });

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
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button variant="ghost" size="sm" className="h-8 w-8 p-0">
                        <MoreHorizontal className="h-4 w-4" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuItem asChild>
                        <Link href={`/dashboard/clients/${client.id}`}>
                          <Eye className="mr-2 h-4 w-4" />
                          View
                        </Link>
                      </DropdownMenuItem>
                      {!client.isActive && (
                        <>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem
                            onClick={() =>
                              resendMutation.mutate({ id: client.id! })
                            }
                            disabled={resendMutation.isPending}
                          >
                            <Send className="mr-2 h-4 w-4" />
                            Wyślij zaproszenie ponownie
                          </DropdownMenuItem>
                        </>
                      )}
                    </DropdownMenuContent>
                  </DropdownMenu>
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
