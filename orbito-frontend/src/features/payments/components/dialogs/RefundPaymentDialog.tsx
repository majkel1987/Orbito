"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/shared/ui/dialog";
import { Button } from "@/shared/ui/button";
import { Input } from "@/shared/ui/input";
import { Label } from "@/shared/ui/label";
import { Textarea } from "@/shared/ui/textarea";
import { useRefundPayment } from "../../hooks/usePaymentMutations";
import {
  refundPaymentSchema,
  type RefundPaymentInput,
} from "../../schemas/payment.schemas";

interface RefundPaymentDialogProps {
  isOpen: boolean;
  onClose: () => void;
  paymentId: string;
  clientId: string;
  maxAmount: number;
  currency: string;
}

export function RefundPaymentDialog({
  isOpen,
  onClose,
  paymentId,
  clientId,
  maxAmount,
  currency,
}: RefundPaymentDialogProps) {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<RefundPaymentInput>({
    resolver: zodResolver(refundPaymentSchema),
    defaultValues: {
      paymentId,
      clientId,
      amount: maxAmount,
      currency,
      reason: "",
    },
  });

  const refundMutation = useRefundPayment();

  const onSubmit = async (data: RefundPaymentInput) => {
    await refundMutation.mutateAsync({
      id: paymentId,
      params: {
        clientId: data.clientId,
      },
      data: {
        paymentId: data.paymentId,
        clientId: data.clientId,
        amount: data.amount,
        currency: data.currency,
        reason: data.reason,
      },
    });
    reset();
    onClose();
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Refund Payment</DialogTitle>
          <DialogDescription>
            Process a refund for this payment. Maximum refund amount:{" "}
            {maxAmount.toFixed(2)} {currency}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {/* Amount */}
          <div className="space-y-2">
            <Label htmlFor="amount">Refund Amount *</Label>
            <Input
              id="amount"
              type="number"
              step="0.01"
              max={maxAmount}
              {...register("amount", { valueAsNumber: true })}
              placeholder="0.00"
            />
            {errors.amount && (
              <p className="text-sm text-red-500">{errors.amount.message}</p>
            )}
          </div>

          {/* Currency (read-only, matches payment currency) */}
          <div className="space-y-2">
            <Label htmlFor="currency">Currency</Label>
            <Input
              id="currency"
              {...register("currency")}
              readOnly
              disabled
              className="bg-muted"
            />
          </div>

          {/* Reason */}
          <div className="space-y-2">
            <Label htmlFor="reason">Reason *</Label>
            <Textarea
              id="reason"
              {...register("reason")}
              placeholder="Please provide a reason for the refund..."
              rows={4}
            />
            {errors.reason && (
              <p className="text-sm text-red-500">{errors.reason.message}</p>
            )}
          </div>

          {/* Actions */}
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit" variant="destructive" disabled={isSubmitting}>
              {isSubmitting ? "Processing..." : "Refund Payment"}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
