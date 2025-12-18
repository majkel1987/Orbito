"use client";

import { useState } from "react";
import { useGetApiSubscriptionsId } from "@/core/api/generated/subscriptions/subscriptions";
import { Card } from "@/shared/ui/card";
import { Badge } from "@/shared/ui/badge";
import { Skeleton } from "@/shared/ui/skeleton";
import { Button } from "@/shared/ui/button";
import { formatCurrency, formatDate } from "@/shared/lib/formatters";
import {
  getSubscriptionStatusVariant,
  getClientDisplayName,
  type SubscriptionDto,
} from "../types/subscription.types";
import { CancelSubscriptionDialog } from "./dialogs/CancelSubscriptionDialog";
import { SuspendSubscriptionDialog } from "./dialogs/SuspendSubscriptionDialog";
import { ResumeSubscriptionDialog } from "./dialogs/ResumeSubscriptionDialog";

interface SubscriptionDetailProps {
  subscriptionId: string;
}

export function SubscriptionDetail({ subscriptionId }: SubscriptionDetailProps) {
  const [showCancelDialog, setShowCancelDialog] = useState(false);
  const [showSuspendDialog, setShowSuspendDialog] = useState(false);
  const [showResumeDialog, setShowResumeDialog] = useState(false);

  const { data, isLoading, error } = useGetApiSubscriptionsId(subscriptionId, {
    includeDetails: true,
  }) as {
    data: SubscriptionDto | undefined;
    isLoading: boolean;
    error: Error | null;
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Card className="p-6">
          <Skeleton className="h-8 w-48 mb-4" />
          <Skeleton className="h-40" />
        </Card>
        <Card className="p-6">
          <Skeleton className="h-40" />
        </Card>
      </div>
    );
  }

  if (error) {
    return (
      <Card className="p-6">
        <div className="text-red-500">Error loading subscription: {error.message}</div>
      </Card>
    );
  }

  if (!data) {
    return (
      <Card className="p-6">
        <div className="text-muted-foreground">Subscription not found</div>
      </Card>
    );
  }

  const canCancel = data.status === "Active" || data.status === "Trial";
  const canSuspend = data.status === "Active" || data.status === "Trial";
  const canResume = data.status === "Suspended";

  return (
    <>
      <div className="space-y-6">
        {/* Status and Actions */}
        <Card className="p-6">
          <div className="flex items-start justify-between mb-6">
            <div>
              <h2 className="text-2xl font-semibold mb-2">Subscription Status</h2>
              <Badge variant={getSubscriptionStatusVariant(data.status)}>
                {data.status}
              </Badge>
              {data.isInTrial && (
                <Badge variant="secondary" className="ml-2">
                  In Trial
                </Badge>
              )}
            </div>
            <div className="flex gap-2">
              {canResume && (
                <Button
                  variant="outline"
                  onClick={() => setShowResumeDialog(true)}
                >
                  Resume
                </Button>
              )}
              {canSuspend && (
                <Button
                  variant="outline"
                  onClick={() => setShowSuspendDialog(true)}
                >
                  Suspend
                </Button>
              )}
              {canCancel && (
                <Button
                  variant="destructive"
                  onClick={() => setShowCancelDialog(true)}
                >
                  Cancel Subscription
                </Button>
              )}
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <h3 className="font-semibold mb-2">Client</h3>
              <div className="space-y-1">
                <div className="font-medium">{getClientDisplayName(data)}</div>
                {data.clientEmail && (
                  <div className="text-sm text-muted-foreground">{data.clientEmail}</div>
                )}
              </div>
            </div>

            <div>
              <h3 className="font-semibold mb-2">Plan</h3>
              <div className="space-y-1">
                <div className="font-medium">{data.planName || "Unknown Plan"}</div>
                {data.planDescription && (
                  <div className="text-sm text-muted-foreground">{data.planDescription}</div>
                )}
              </div>
            </div>
          </div>
        </Card>

        {/* Billing Information */}
        <Card className="p-6">
          <h2 className="text-2xl font-semibold mb-4">Billing Information</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <h3 className="text-sm font-medium text-muted-foreground mb-1">Amount</h3>
              <div className="text-2xl font-bold">
                {formatCurrency(data.amount, data.currency)}
              </div>
              <div className="text-sm text-muted-foreground">
                per {data.billingPeriodType}
              </div>
            </div>

            <div>
              <h3 className="text-sm font-medium text-muted-foreground mb-1">
                Next Billing Date
              </h3>
              <div className="text-lg font-semibold">
                {formatDate(data.nextBillingDate)}
              </div>
            </div>

            <div>
              <h3 className="text-sm font-medium text-muted-foreground mb-1">Start Date</h3>
              <div className="text-lg">{formatDate(data.startDate)}</div>
            </div>

            {data.endDate && (
              <div>
                <h3 className="text-sm font-medium text-muted-foreground mb-1">End Date</h3>
                <div className="text-lg">{formatDate(data.endDate)}</div>
              </div>
            )}

            {data.isInTrial && data.trialEndDate && (
              <div>
                <h3 className="text-sm font-medium text-muted-foreground mb-1">
                  Trial Ends
                </h3>
                <div className="text-lg">{formatDate(data.trialEndDate)}</div>
              </div>
            )}

            {data.cancelledAt && (
              <div>
                <h3 className="text-sm font-medium text-muted-foreground mb-1">
                  Cancelled At
                </h3>
                <div className="text-lg">{formatDate(data.cancelledAt)}</div>
              </div>
            )}
          </div>
        </Card>

        {/* Payment History */}
        {(data.paymentCount !== undefined || data.totalPaid !== undefined) && (
          <Card className="p-6">
            <h2 className="text-2xl font-semibold mb-4">Payment Summary</h2>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              {data.paymentCount !== undefined && (
                <div>
                  <h3 className="text-sm font-medium text-muted-foreground mb-1">
                    Total Payments
                  </h3>
                  <div className="text-2xl font-bold">{data.paymentCount}</div>
                </div>
              )}

              {data.totalPaid !== undefined && (
                <div>
                  <h3 className="text-sm font-medium text-muted-foreground mb-1">
                    Total Paid
                  </h3>
                  <div className="text-2xl font-bold">
                    {formatCurrency(data.totalPaid, data.currency)}
                  </div>
                </div>
              )}

              {data.lastPaymentDate && (
                <div>
                  <h3 className="text-sm font-medium text-muted-foreground mb-1">
                    Last Payment
                  </h3>
                  <div className="text-lg">{formatDate(data.lastPaymentDate)}</div>
                </div>
              )}
            </div>
          </Card>
        )}
      </div>

      {/* Dialogs */}
      <CancelSubscriptionDialog
        subscriptionId={subscriptionId}
        clientId={data.clientId}
        open={showCancelDialog}
        onOpenChange={setShowCancelDialog}
      />
      <SuspendSubscriptionDialog
        subscriptionId={subscriptionId}
        clientId={data.clientId}
        open={showSuspendDialog}
        onOpenChange={setShowSuspendDialog}
      />
      <ResumeSubscriptionDialog
        subscriptionId={subscriptionId}
        clientId={data.clientId}
        open={showResumeDialog}
        onOpenChange={setShowResumeDialog}
      />
    </>
  );
}
