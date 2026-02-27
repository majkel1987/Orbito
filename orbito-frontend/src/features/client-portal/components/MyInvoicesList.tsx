"use client";

import { formatCurrency, formatDate } from "@/shared/lib/formatters";
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
import { Download } from "lucide-react";
import type { PaymentDto } from "@/core/api/generated/models";

const PAYMENT_STATUS_VARIANT: Record<
  string,
  "default" | "secondary" | "destructive" | "outline"
> = {
  Completed: "default",
  Pending: "secondary",
  Failed: "destructive",
  Refunded: "outline",
  Processing: "secondary",
};

interface MyInvoicesListProps {
  invoices: PaymentDto[];
}

export function MyInvoicesListSkeleton() {
  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Data</TableHead>
            <TableHead>Kwota</TableHead>
            <TableHead>Status</TableHead>
            <TableHead className="w-12" />
          </TableRow>
        </TableHeader>
        <TableBody>
          {[1, 2, 3].map((i) => (
            <TableRow key={i}>
              <TableCell>
                <Skeleton className="h-4 w-24" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-20" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-16" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-6" />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}

export function MyInvoicesList({ invoices }: MyInvoicesListProps) {
  if (invoices.length === 0) {
    return (
      <div className="rounded-md border px-4 py-8 text-center text-muted-foreground">
        Brak historii płatności.
      </div>
    );
  }

  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Data</TableHead>
            <TableHead>Kwota</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Metoda</TableHead>
            <TableHead className="w-12 text-right">PDF</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {invoices.map((invoice) => {
            const status = invoice.status ?? "Unknown";
            const badgeVariant = PAYMENT_STATUS_VARIANT[status] ?? "outline";
            return (
              <TableRow key={invoice.id}>
                <TableCell className="text-sm">
                  {invoice.createdAt ? formatDate(invoice.createdAt) : "—"}
                </TableCell>
                <TableCell className="font-medium">
                  {formatCurrency(invoice.amount ?? 0, invoice.currency ?? "PLN")}
                </TableCell>
                <TableCell>
                  <Badge variant={badgeVariant}>{status}</Badge>
                </TableCell>
                <TableCell className="text-sm text-muted-foreground">
                  {invoice.paymentMethod ?? "—"}
                </TableCell>
                <TableCell className="text-right">
                  <button
                    aria-label="Pobierz fakturę PDF"
                    className="inline-flex h-8 w-8 items-center justify-center rounded-md text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                    disabled
                    title="PDF download coming soon"
                  >
                    <Download className="h-4 w-4" />
                  </button>
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </div>
  );
}
