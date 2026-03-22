"use client";

import { useState } from "react";
import { formatCurrency, formatDate } from "@/shared/lib/formatters";
import { Badge } from "@/shared/ui/badge";
import { Button } from "@/shared/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { Skeleton } from "@/shared/ui/skeleton";
import { CreditCard } from "lucide-react";
import type { SubscriptionDto } from "@/core/api/generated/models";
import { PaySubscriptionDialog } from "./PaySubscriptionDialog";

const STATUS_VARIANT: Record<
  string,
  "default" | "secondary" | "destructive" | "outline"
> = {
  Active: "default",
  Suspended: "secondary",
  Cancelled: "destructive",
  Expired: "outline",
};

interface MySubscriptionsCardProps {
  subscriptions: SubscriptionDto[];
}

export function MySubscriptionsCardSkeleton() {
  return (
    <div className="space-y-4">
      {[1, 2].map((i) => (
        <Card key={i}>
          <CardHeader>
            <Skeleton className="h-6 w-40" />
          </CardHeader>
          <CardContent className="space-y-2">
            <Skeleton className="h-4 w-32" />
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-4 w-36" />
          </CardContent>
        </Card>
      ))}
    </div>
  );
}

export function MySubscriptionsCard({ subscriptions }: MySubscriptionsCardProps) {
  const [payDialogOpen, setPayDialogOpen] = useState(false);
  const [selectedSubscription, setSelectedSubscription] = useState<SubscriptionDto | null>(null);

  const handlePayClick = (subscription: SubscriptionDto) => {
    setSelectedSubscription(subscription);
    setPayDialogOpen(true);
  };

  if (subscriptions.length === 0) {
    return (
      <Card>
        <CardContent className="flex items-center justify-center py-8 text-muted-foreground">
          Nie masz aktywnych subskrypcji.
        </CardContent>
      </Card>
    );
  }

  return (
    <>
      <div className="space-y-4">
        {subscriptions.map((sub) => {
          const status = sub.status ?? "Unknown";
          const badgeVariant = STATUS_VARIANT[status] ?? "outline";
          // Show pay button for active subscriptions
          const canPay = status === "Active" || status === "Suspended";

          return (
            <Card key={sub.id}>
              <CardHeader className="pb-2">
                <div className="flex items-center justify-between">
                  <CardTitle className="text-lg">
                    {sub.planName ?? "Plan"}
                  </CardTitle>
                  <Badge variant={badgeVariant}>{status}</Badge>
                </div>
              </CardHeader>
              <CardContent className="space-y-3">
                <div className="space-y-1 text-sm text-muted-foreground">
                  <p>
                    <span className="font-medium text-foreground">Kwota: </span>
                    {formatCurrency(sub.amount ?? 0, sub.currency ?? "PLN")} /{" "}
                    {sub.billingPeriodType ?? ""}
                  </p>
                  {sub.nextBillingDate && (
                    <p>
                      <span className="font-medium text-foreground">
                        Następne odnowienie:{" "}
                      </span>
                      {formatDate(sub.nextBillingDate)}
                    </p>
                  )}
                  {sub.startDate && (
                    <p>
                      <span className="font-medium text-foreground">
                        Aktywna od:{" "}
                      </span>
                      {formatDate(sub.startDate)}
                    </p>
                  )}
                  {sub.isInTrial && sub.trialEndDate && (
                    <p className="text-amber-600">
                      Okres próbny do: {formatDate(sub.trialEndDate)}
                    </p>
                  )}
                </div>
                {canPay && (
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => handlePayClick(sub)}
                    className="w-full sm:w-auto"
                  >
                    <CreditCard className="mr-2 h-4 w-4" />
                    Opłać
                  </Button>
                )}
              </CardContent>
            </Card>
          );
        })}
      </div>

      <PaySubscriptionDialog
        subscription={selectedSubscription}
        open={payDialogOpen}
        onOpenChange={setPayDialogOpen}
      />
    </>
  );
}
