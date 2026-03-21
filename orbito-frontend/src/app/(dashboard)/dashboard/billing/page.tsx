"use client";

import { CreditCard, Clock, CheckCircle } from "lucide-react";
import { useProviderSubscription } from "@/features/billing/hooks/useProviderSubscription";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/ui/card";
import { Badge } from "@/shared/ui/badge";
import { Skeleton } from "@/shared/ui/skeleton";
import { cn } from "@/shared/lib/utils";

export default function BillingPage() {
  const { subscription, isLoading, isTrial, isActive, isExpired, daysRemaining, planName } =
    useProviderSubscription();

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-bold">Subskrypcja</h1>
          <p className="text-muted-foreground">Zarządzaj swoją subskrypcją platformy Orbito</p>
        </div>
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-48" />
            <Skeleton className="h-4 w-64" />
          </CardHeader>
          <CardContent>
            <Skeleton className="h-32 w-full" />
          </CardContent>
        </Card>
      </div>
    );
  }

  const statusBadge = isTrial ? (
    <Badge variant="outline" className="bg-blue-100 text-blue-800 dark:bg-blue-950 dark:text-blue-200">
      <Clock className="mr-1 h-3 w-3" />
      Okres próbny
    </Badge>
  ) : isActive ? (
    <Badge variant="outline" className="bg-green-100 text-green-800 dark:bg-green-950 dark:text-green-200">
      <CheckCircle className="mr-1 h-3 w-3" />
      Aktywna
    </Badge>
  ) : isExpired ? (
    <Badge variant="destructive">Wygasła</Badge>
  ) : (
    <Badge variant="secondary">Nieznany</Badge>
  );

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Subskrypcja</h1>
        <p className="text-muted-foreground">Zarządzaj swoją subskrypcją platformy Orbito</p>
      </div>

      {/* Current Plan Card */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <CreditCard className="h-5 w-5" />
              Aktualny plan
            </CardTitle>
            {statusBadge}
          </div>
          <CardDescription>
            {isTrial
              ? `Pozostało ${daysRemaining} dni okresu próbnego`
              : isActive
                ? "Twoja subskrypcja jest aktywna"
                : isExpired
                  ? "Twoja subskrypcja wygasła. Opłać, aby odzyskać dostęp."
                  : ""}
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="rounded-lg border p-6">
            <div className="flex items-center justify-between">
              <div>
                <h3 className="text-xl font-semibold">{planName || "Brak planu"}</h3>
                {subscription && (
                  <p className="text-2xl font-bold text-primary">
                    {subscription.planPrice} {subscription.planCurrency}
                    <span className="text-sm font-normal text-muted-foreground">/miesiąc</span>
                  </p>
                )}
              </div>
              {isTrial && (
                <div
                  className={cn(
                    "rounded-lg px-4 py-2 text-center",
                    daysRemaining <= 1
                      ? "bg-red-100 text-red-800 dark:bg-red-950 dark:text-red-200"
                      : daysRemaining <= 5
                        ? "bg-amber-100 text-amber-800 dark:bg-amber-950 dark:text-amber-200"
                        : "bg-blue-100 text-blue-800 dark:bg-blue-950 dark:text-blue-200"
                  )}
                >
                  <p className="text-sm font-medium">Pozostało</p>
                  <p className="text-2xl font-bold">{daysRemaining}</p>
                  <p className="text-xs">dni</p>
                </div>
              )}
            </div>

            {subscription?.trialEndDate && isTrial && (
              <p className="mt-4 text-sm text-muted-foreground">
                Trial kończy się:{" "}
                <span className="font-medium">
                  {new Date(subscription.trialEndDate).toLocaleDateString("pl-PL", {
                    day: "numeric",
                    month: "long",
                    year: "numeric",
                  })}
                </span>
              </p>
            )}
          </div>

          {/* Placeholder for payment - will be implemented in ISSUE 6.4 */}
          {(isTrial || isExpired) && (
            <div className="mt-6 rounded-lg border-2 border-dashed border-muted-foreground/25 p-8 text-center">
              <CreditCard className="mx-auto h-12 w-12 text-muted-foreground/50" />
              <h4 className="mt-4 text-lg font-medium">Płatność zostanie dodana w następnej wersji</h4>
              <p className="mt-2 text-sm text-muted-foreground">
                Formularz płatności Stripe zostanie zaimplementowany w ISSUE 6.4
              </p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
