"use client";

import Link from "next/link";
import { Input } from "@/shared/ui/input";
import { Button } from "@/shared/ui/button";
import { Badge } from "@/shared/ui/badge";
import { Skeleton } from "@/shared/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/ui/table";
import { Pagination } from "@/shared/components/Pagination";
import { formatCurrency, formatDate } from "@/shared/lib/formatters";
import { usePayments } from "../hooks/usePayments";
import {
  getPaymentStatusVariant,
  getPaymentStatusLabel,
} from "../types/payment.types";
import { Search } from "lucide-react";
import { useState, useRef } from "react";

export function PaymentsTable() {
  // Local state for search input (debounced)
  const [localSearch, setLocalSearch] = useState("");
  const debounceTimeout = useRef<NodeJS.Timeout | undefined>(undefined);

  const {
    payments,
    totalCount,
    pageNumber,
    totalPages,
    isLoading,
    error,
    goToPage,
  } = usePayments({
    pageSize: 10,
    searchTerm: localSearch,
  });

  // Debounced search handler
  const handleSearchChange = (value: string) => {
    setLocalSearch(value);

    if (debounceTimeout.current) {
      clearTimeout(debounceTimeout.current);
    }

    debounceTimeout.current = setTimeout(() => {
      // Search will be applied through usePayments hook
    }, 500);
  };

  // Loading state
  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-10 w-full" />
        <Skeleton className="h-[400px] w-full" />
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-red-600">
        Error loading payments: {error instanceof Error ? error.message : "Unknown error"}
      </div>
    );
  }

  // Empty state
  if (!payments.length && !localSearch) {
    return (
      <div className="rounded-lg border border-gray-200 p-12 text-center">
        <p className="text-gray-500">No payments yet</p>
        <p className="mt-2 text-sm text-gray-400">
          Payments will appear here once processed.
        </p>
      </div>
    );
  }

  // Empty search results
  if (!payments.length && localSearch) {
    return (
      <div className="space-y-4">
        {/* Search bar */}
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
          <Input
            placeholder="Search payments..."
            value={localSearch}
            onChange={(e) => handleSearchChange(e.target.value)}
            className="pl-9"
          />
        </div>

        <div className="rounded-lg border border-gray-200 p-12 text-center">
          <p className="text-gray-500">No payments found</p>
          <p className="mt-2 text-sm text-gray-400">
            Try adjusting your search criteria.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Search bar */}
      <div className="relative flex-1">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
        <Input
          placeholder="Search payments..."
          value={localSearch}
          onChange={(e) => handleSearchChange(e.target.value)}
          className="pl-9"
        />
      </div>

      {/* Table */}
      <div className="rounded-lg border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Transaction ID</TableHead>
              <TableHead>Amount</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Payment Method</TableHead>
              <TableHead>Date</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {payments.map((payment) => (
              <TableRow key={payment?.id}>
                <TableCell>
                  <div>
                    <div className="font-medium font-mono text-sm">
                      {payment?.externalTransactionId || payment?.id?.slice(0, 8)}
                    </div>
                    {payment?.failureReason && (
                      <div className="text-xs text-red-500 mt-1">
                        {payment.failureReason}
                      </div>
                    )}
                  </div>
                </TableCell>
                <TableCell className="font-medium">
                  {formatCurrency(payment?.amount ?? 0, payment?.currency ?? "USD")}
                </TableCell>
                <TableCell>
                  <Badge variant={getPaymentStatusVariant(payment?.status ?? "")}>
                    {getPaymentStatusLabel(payment?.status ?? "")}
                  </Badge>
                </TableCell>
                <TableCell>
                  {payment?.paymentMethod || "N/A"}
                </TableCell>
                <TableCell>
                  {formatDate(payment?.createdAt ?? "")}
                </TableCell>
                <TableCell className="text-right">
                  <Link href={`/dashboard/payments/${payment?.id}`}>
                    <Button variant="ghost" size="sm">
                      View
                    </Button>
                  </Link>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <Pagination
          currentPage={pageNumber}
          totalPages={totalPages}
          onPageChange={goToPage}
        />
      )}

      {/* Results info */}
      <p className="text-sm text-gray-500 text-center">
        Showing {payments.length} of {totalCount} payments
      </p>
    </div>
  );
}
