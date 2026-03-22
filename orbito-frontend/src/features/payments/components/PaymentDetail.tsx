"use client";

import { useState } from "react";
import { Button } from "@/shared/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { Badge } from "@/shared/ui/badge";
import { Skeleton } from "@/shared/ui/skeleton";
import { useGetApiPaymentId } from "@/core/api/generated/payment/payment";
import type { PaymentDto } from "@/core/api/generated/models/paymentDto";
import { formatCurrency } from "@/shared/lib/formatters";
import { formatDate } from "@/shared/lib/formatters";
import {
  getPaymentStatusVariant,
  getPaymentStatusLabel,
} from "../types/payment.types";
import { RefundPaymentDialog } from "./dialogs/RefundPaymentDialog";

interface PaymentDetailProps {
  paymentId: string;
  clientId?: string;
}

export function PaymentDetail({ paymentId, clientId }: PaymentDetailProps) {
  const [isRefundDialogOpen, setIsRefundDialogOpen] = useState(false);

  const { data, isLoading, error } = useGetApiPaymentId(paymentId, clientId ? { clientId } : undefined) as {
    data: PaymentDto | undefined;
    isLoading: boolean;
    error: Error | null;
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-64" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-red-500">
        Error loading payment: {error.message}
      </div>
    );
  }

  if (!data) {
    return <div>Payment not found</div>;
  }

  const canRefund = data.status === "Completed" || data.status === "1";

  return (
    <>
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <div>
            <CardTitle>Payment Details</CardTitle>
            <p className="text-sm text-muted-foreground">
              Payment ID: {data.id}
            </p>
          </div>
          <Badge variant={getPaymentStatusVariant(data.status || "")}>
            {getPaymentStatusLabel(data.status || "")}
          </Badge>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Payment Information */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-sm font-medium text-muted-foreground">
                Amount
              </p>
              <p className="text-lg font-bold">
                {formatCurrency(data.amount || 0, data.currency || "PLN")}
              </p>
            </div>

            <div>
              <p className="text-sm font-medium text-muted-foreground">
                Payment Date
              </p>
              <p className="text-lg">
                {data.processedAt ? formatDate(data.processedAt) : data.createdAt ? formatDate(data.createdAt) : "N/A"}
              </p>
            </div>

            <div>
              <p className="text-sm font-medium text-muted-foreground">
                Payment Method
              </p>
              <p className="text-lg">{data.paymentMethod || "N/A"}</p>
            </div>

            <div>
              <p className="text-sm font-medium text-muted-foreground">
                External Transaction ID
              </p>
              <p className="text-lg">
                {data.externalTransactionId || "N/A"}
              </p>
            </div>

            {data.externalPaymentId && (
              <div>
                <p className="text-sm font-medium text-muted-foreground">
                  External Payment ID
                </p>
                <p className="text-lg">{data.externalPaymentId}</p>
              </div>
            )}
          </div>

          {/* Actions */}
          <div className="flex gap-2 pt-4 border-t">
            {canRefund && (
              <Button
                variant="destructive"
                onClick={() => setIsRefundDialogOpen(true)}
              >
                Refund Payment
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Refund Dialog */}
      {canRefund && (
        <RefundPaymentDialog
          isOpen={isRefundDialogOpen}
          onClose={() => setIsRefundDialogOpen(false)}
          paymentId={data.id || ""}
          clientId={data.clientId || ""}
          maxAmount={data.amount || 0}
          currency={data.currency || "PLN"}
        />
      )}
    </>
  );
}
