"use client";

import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/shared/ui/dialog";
import { Alert, AlertDescription } from "@/shared/ui/alert";
import { Loader2, AlertCircle } from "lucide-react";
import { StripeProvider } from "@/core/providers/StripeProvider";
import { PaymentForm } from "./PaymentForm";
import axiosInstance from "@/core/api/client";
import type { SubscriptionDto } from "@/core/api/generated/models";

// Response type from CreatePaymentIntent endpoint
interface CreatePaymentIntentResponse {
  clientSecret: string;
  paymentIntentId: string;
  amount: number;
  currency: string;
}

// Hook for creating payment intent (manual until api:generate runs)
function useCreatePaymentIntent() {
  return useMutation({
    mutationFn: async (subscriptionId: string): Promise<CreatePaymentIntentResponse> => {
      const response = await axiosInstance.post<CreatePaymentIntentResponse>(
        "/api/portal/payments/create-intent",
        { subscriptionId }
      );
      return response.data;
    },
  });
}

interface PaySubscriptionDialogProps {
  subscription: SubscriptionDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

/**
 * Dialog for paying a subscription using Stripe Elements.
 * Handles:
 * 1. Creating PaymentIntent on backend
 * 2. Collecting card details via Stripe Elements (PCI DSS compliant)
 * 3. Confirming payment with Stripe
 */
export function PaySubscriptionDialog({
  subscription,
  open,
  onOpenChange,
}: PaySubscriptionDialogProps) {
  const [step, setStep] = useState<"loading" | "form" | "error">("loading");
  const [paymentData, setPaymentData] = useState<CreatePaymentIntentResponse | null>(null);

  const createIntentMutation = useCreatePaymentIntent();

  // When dialog opens, create PaymentIntent
  const handleOpenChange = (isOpen: boolean) => {
    if (isOpen && subscription?.id) {
      setStep("loading");
      setPaymentData(null);

      createIntentMutation.mutate(subscription.id, {
        onSuccess: (data) => {
          setPaymentData(data);
          setStep("form");
        },
        onError: () => {
          setStep("error");
        },
      });
    }
    onOpenChange(isOpen);
  };

  const handleCancel = () => {
    onOpenChange(false);
  };

  const handleRetry = () => {
    if (subscription?.id) {
      setStep("loading");
      createIntentMutation.mutate(subscription.id, {
        onSuccess: (data) => {
          setPaymentData(data);
          setStep("form");
        },
        onError: () => {
          setStep("error");
        },
      });
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-md" showCloseButton={step !== "loading"}>
        <DialogHeader>
          <DialogTitle>Opłać subskrypcję</DialogTitle>
          <DialogDescription>
            {subscription?.planName ?? "Subskrypcja"}
          </DialogDescription>
        </DialogHeader>

        {step === "loading" && (
          <div className="flex flex-col items-center justify-center py-8 gap-4">
            <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
            <p className="text-sm text-muted-foreground">
              Przygotowywanie płatności...
            </p>
          </div>
        )}

        {step === "error" && (
          <div className="space-y-4">
            <Alert variant="destructive">
              <AlertCircle className="h-4 w-4" />
              <AlertDescription>
                {createIntentMutation.error?.message ??
                  "Nie udało się przygotować płatności. Spróbuj ponownie."}
              </AlertDescription>
            </Alert>
            <div className="flex gap-3">
              <button
                onClick={handleCancel}
                className="flex-1 rounded-md border px-4 py-2 text-sm hover:bg-muted"
              >
                Anuluj
              </button>
              <button
                onClick={handleRetry}
                className="flex-1 rounded-md bg-primary px-4 py-2 text-sm text-primary-foreground hover:bg-primary/90"
              >
                Spróbuj ponownie
              </button>
            </div>
          </div>
        )}

        {step === "form" && paymentData && (
          <StripeProvider clientSecret={paymentData.clientSecret}>
            <PaymentForm
              amount={paymentData.amount}
              currency={paymentData.currency}
              onCancel={handleCancel}
            />
          </StripeProvider>
        )}
      </DialogContent>
    </Dialog>
  );
}
