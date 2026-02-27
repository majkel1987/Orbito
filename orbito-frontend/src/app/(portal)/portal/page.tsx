"use client";

import {
  MySubscriptionsCard,
  MySubscriptionsCardSkeleton,
} from "@/features/client-portal/components/MySubscriptionsCard";
import {
  MyInvoicesList,
  MyInvoicesListSkeleton,
} from "@/features/client-portal/components/MyInvoicesList";
import { useMySubscriptions, useMyInvoices } from "@/features/client-portal/hooks/usePortal";

function getErrorMessage(error: unknown): string {
  if (error instanceof Error) return error.message;
  return "Nieznany błąd";
}

export default function PortalPage() {
  const {
    data: subscriptions,
    isLoading: isLoadingSubscriptions,
    error: subscriptionsError,
  } = useMySubscriptions();

  const {
    data: invoices,
    isLoading: isLoadingInvoices,
    error: invoicesError,
  } = useMyInvoices();

  return (
    <div className="space-y-8">
      {/* Page header */}
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Mój Portal</h1>
        <p className="text-muted-foreground">
          Zarządzaj swoimi subskrypcjami i historią płatności.
        </p>
      </div>

      {/* Subscriptions section */}
      <section aria-labelledby="subscriptions-heading">
        <h2
          id="subscriptions-heading"
          className="mb-4 text-lg font-semibold"
        >
          Moje subskrypcje
        </h2>

        {isLoadingSubscriptions && <MySubscriptionsCardSkeleton />}

        {subscriptionsError && (
          <div className="rounded-md border border-destructive/50 bg-destructive/10 px-4 py-3 text-sm text-destructive">
            Błąd podczas ładowania subskrypcji: {getErrorMessage(subscriptionsError)}
          </div>
        )}

        {!isLoadingSubscriptions && !subscriptionsError && (
          <MySubscriptionsCard subscriptions={subscriptions ?? []} />
        )}
      </section>

      {/* Invoices section */}
      <section aria-labelledby="invoices-heading">
        <h2
          id="invoices-heading"
          className="mb-4 text-lg font-semibold"
        >
          Historia płatności
        </h2>

        {isLoadingInvoices && <MyInvoicesListSkeleton />}

        {invoicesError && (
          <div className="rounded-md border border-destructive/50 bg-destructive/10 px-4 py-3 text-sm text-destructive">
            Błąd podczas ładowania płatności: {getErrorMessage(invoicesError)}
          </div>
        )}

        {!isLoadingInvoices && !invoicesError && (
          <MyInvoicesList invoices={invoices ?? []} />
        )}
      </section>
    </div>
  );
}
