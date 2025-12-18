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
import { useSubscriptions } from "../hooks/useSubscriptions";
import {
  getSubscriptionStatusVariant,
  getClientDisplayName,
} from "../types/subscription.types";
import { Search, X } from "lucide-react";
import { useState, useRef, useEffect } from "react";

export function SubscriptionsTable() {
  const {
    subscriptions,
    pagination,
    isLoading,
    error,
    searchTerm,
    goToPage,
    setSearch,
    clearFilters,
    hasActiveFilters,
  } = useSubscriptions();

  // Local state for search input (debounced)
  const [localSearch, setLocalSearch] = useState(searchTerm);
  const debounceTimeout = useRef<NodeJS.Timeout | undefined>(undefined);

  // Update local search when URL search param changes
  useEffect(() => {
    setLocalSearch(searchTerm);
  }, [searchTerm]);

  // Debounced search handler
  const handleSearchChange = (value: string) => {
    setLocalSearch(value);

    if (debounceTimeout.current) {
      clearTimeout(debounceTimeout.current);
    }

    debounceTimeout.current = setTimeout(() => {
      setSearch(value);
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
        Error loading subscriptions: {error.message}
      </div>
    );
  }

  // Empty state
  if (!subscriptions.length && !hasActiveFilters) {
    return (
      <div className="rounded-lg border border-gray-200 p-12 text-center">
        <p className="text-gray-500">No subscriptions yet</p>
        <p className="mt-2 text-sm text-gray-400">
          Subscriptions will appear here once clients subscribe to plans.
        </p>
      </div>
    );
  }

  // Empty search results
  if (!subscriptions.length && hasActiveFilters) {
    return (
      <div className="space-y-4">
        {/* Search bar */}
        <div className="flex gap-2">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
            <Input
              placeholder="Search subscriptions..."
              value={localSearch}
              onChange={(e) => handleSearchChange(e.target.value)}
              className="pl-9"
            />
          </div>
          {hasActiveFilters && (
            <Button variant="ghost" onClick={clearFilters}>
              <X className="mr-2 h-4 w-4" />
              Clear
            </Button>
          )}
        </div>

        <div className="rounded-lg border border-gray-200 p-12 text-center">
          <p className="text-gray-500">No subscriptions found</p>
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
      <div className="flex gap-2">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
          <Input
            placeholder="Search subscriptions..."
            value={localSearch}
            onChange={(e) => handleSearchChange(e.target.value)}
            className="pl-9"
          />
        </div>
        {hasActiveFilters && (
          <Button variant="ghost" onClick={clearFilters}>
            <X className="mr-2 h-4 w-4" />
            Clear
          </Button>
        )}
      </div>

      {/* Table */}
      <div className="rounded-lg border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Client</TableHead>
              <TableHead>Plan</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Amount</TableHead>
              <TableHead>Next Billing</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {subscriptions.map((subscription) => (
              <TableRow key={subscription.id}>
                <TableCell>
                  <div>
                    <div className="font-medium">
                      {getClientDisplayName(subscription)}
                    </div>
                    {subscription.clientEmail && (
                      <div className="text-sm text-gray-500">
                        {subscription.clientEmail}
                      </div>
                    )}
                  </div>
                </TableCell>
                <TableCell>
                  {subscription.planName || "Unknown Plan"}
                </TableCell>
                <TableCell>
                  <Badge
                    variant={getSubscriptionStatusVariant(subscription.status)}
                  >
                    {subscription.status}
                  </Badge>
                  {subscription.isInTrial && (
                    <Badge variant="outline" className="ml-2">
                      Trial
                    </Badge>
                  )}
                </TableCell>
                <TableCell>
                  {formatCurrency(subscription.amount, subscription.currency)}
                </TableCell>
                <TableCell>
                  {formatDate(subscription.nextBillingDate)}
                </TableCell>
                <TableCell className="text-right">
                  <Button variant="ghost" size="sm" asChild>
                    <Link href={`/dashboard/subscriptions/${subscription.id}`}>
                      View
                    </Link>
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {/* Pagination */}
      {pagination && pagination.totalPages > 1 && (
        <Pagination
          currentPage={pagination.currentPage}
          totalPages={pagination.totalPages}
          onPageChange={goToPage}
        />
      )}
    </div>
  );
}
