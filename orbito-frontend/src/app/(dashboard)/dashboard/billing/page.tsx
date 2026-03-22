"use client";

import { useState } from "react";
import { CreditCard, Clock, CheckCircle } from "lucide-react";
import { useProviderSubscription } from "@/features/billing/hooks/useProviderSubscription";
import { ProviderPaymentDialog } from "@/features/billing/components/ProviderPaymentDialog";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/ui/card";
import { Badge } from "@/shared/ui/badge";
import { Button } from "@/shared/ui/button";
import { Skeleton } from "@/shared/ui/skeleton";
import { cn } from "@/shared/lib/utils";

export default function BillingPage() {
  const { subscription, isLoading, isTrial, isActive, isExpired, daysRemaining, planName } =
    useProviderSubscription();
  const [paymentDialogOpen, setPaymentDialogOpen] = useState(false);

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

          {/* Payment CTA for trial or expired subscriptions */}
          {(isTrial || isExpired) && (
            <div className="mt-6">
              <Button
                size="lg"
                className="w-full"
                onClick={() => setPaymentDialogOpen(true)}
              >
                <CreditCard className="mr-2 h-5 w-5" />
                {isExpired ? "Odnów subskrypcję" : "Opłać teraz i aktywuj"}
              </Button>
              {isTrial && (
                <p className="mt-2 text-center text-sm text-muted-foreground">
                  Opłacenie teraz aktywuje pełną subskrypcję natychmiast.
                  Okres próbny zostanie zakończony.
                </p>
              )}
            </div>
          )}

          {/* Show paid until date for active subscriptions */}
          {isActive && subscription?.paidUntil && (
            <div className="mt-6 rounded-lg border border-green-200 bg-green-50 p-4 dark:border-green-800 dark:bg-green-950">
              <p className="text-sm text-green-800 dark:text-green-200">
                Subskrypcja opłacona do:{" "}
                <span className="font-medium">
                  {new Date(subscription.paidUntil).toLocaleDateString("pl-PL", {
                    day: "numeric",
                    month: "long",
                    year: "numeric",
                  })}
                </span>
              </p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Payment Dialog */}
      {subscription && (
        <ProviderPaymentDialog
          open={paymentDialogOpen}
          onOpenChange={setPaymentDialogOpen}
          platformPlanId={subscription.platformPlanId}
          planName={planName || "Plan"}
          planPrice={subscription.planPrice}
          planCurrency={subscription.planCurrency}
        />
      )}
    </div>
  );
}
