"use client";

import { useState } from "react";
import { PaymentElement, useStripe, useElements } from "@stripe/react-stripe-js";
import { usePostApiProviderBillingCreatePaymentIntent } from "@/core/api/generated/provider-billing/provider-billing";
import { StripeProvider } from "@/core/providers/StripeProvider";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/shared/ui/dialog";
import { Button } from "@/shared/ui/button";
import { Alert, AlertDescription } from "@/shared/ui/alert";
import { Skeleton } from "@/shared/ui/skeleton";
import { Loader2, CreditCard, AlertCircle } from "lucide-react";
import { formatCurrency } from "@/shared/lib/formatters";

interface ProviderPaymentDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  platformPlanId?: string;
  planName: string;
  planPrice: number;
  planCurrency: string;
}

/**
 * Dialog for Provider platform subscription payment.
 * Uses Stripe Payment Element for PCI DSS compliant card collection.
 */
export function ProviderPaymentDialog({
  open,
  onOpenChange,
  platformPlanId,
  planName,
  planPrice,
  planCurrency,
}: ProviderPaymentDialogProps) {
  const [clientSecret, setClientSecret] = useState<string | null>(null);
  const [paymentError, setPaymentError] = useState<string | null>(null);

  const { mutate: createPaymentIntent, isPending: isCreating } =
    usePostApiProviderBillingCreatePaymentIntent({
      mutation: {
        onSuccess: (data) => {
          setClientSecret(data.clientSecret);
          setPaymentError(null);
        },
        onError: (error) => {
          const message = error instanceof Error ? error.message : "Nie udało się utworzyć płatności";
          setPaymentError(message);
        },
      },
    });

  const handleOpenChange = (newOpen: boolean) => {
    if (newOpen && !clientSecret && !isCreating) {
      // Create payment intent when dialog opens
      createPaymentIntent({
        data: { platformPlanId: platformPlanId || null },
      });
    }
    if (!newOpen) {
      // Reset state when closing
      setClientSecret(null);
      setPaymentError(null);
    }
    onOpenChange(newOpen);
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <CreditCard className="h-5 w-5" />
            Opłać subskrypcję
          </DialogTitle>
          <DialogDescription>
            Opłać plan {planName}, aby kontynuować korzystanie z platformy.
          </DialogDescription>
        </DialogHeader>

        {paymentError && (
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>{paymentError}</AlertDescription>
          </Alert>
        )}

        {isCreating && <PaymentFormSkeleton />}

        {clientSecret && !isCreating && (
          <StripeProvider clientSecret={clientSecret}>
            <PaymentFormContent
              amount={planPrice}
              currency={planCurrency}
              onCancel={() => onOpenChange(false)}
            />
          </StripeProvider>
        )}

        {!clientSecret && !isCreating && !paymentError && (
          <div className="flex justify-center py-8">
            <Button
              onClick={() =>
                createPaymentIntent({
                  data: { platformPlanId: platformPlanId || null },
                })
              }
            >
              Rozpocznij płatność
            </Button>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}

interface PaymentFormContentProps {
  amount: number;
  currency: string;
  onCancel: () => void;
}

function PaymentFormContent({ amount, currency, onCancel }: PaymentFormContentProps) {
  const stripe = useStripe();
  const elements = useElements();
  const [isProcessing, setIsProcessing] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!stripe || !elements) {
      return;
    }

    setIsProcessing(true);
    setErrorMessage(null);

    try {
      const { error } = await stripe.confirmPayment({
        elements,
        confirmParams: {
          return_url: `${window.location.origin}/dashboard/billing/payment-success`,
        },
      });

      if (error) {
        if (error.type === "card_error" || error.type === "validation_error") {
          setErrorMessage(error.message ?? "Wystąpił błąd podczas płatności.");
        } else {
          setErrorMessage("Wystąpił nieoczekiwany błąd. Spróbuj ponownie.");
        }
      }
    } catch {
      setErrorMessage("Wystąpił błąd połączenia. Sprawdź internet i spróbuj ponownie.");
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Amount summary */}
      <div className="rounded-lg border bg-muted/50 p-4">
        <div className="flex items-center justify-between">
          <span className="text-sm text-muted-foreground">Do zapłaty:</span>
          <span className="text-2xl font-bold">
            {formatCurrency(amount, currency)}
          </span>
        </div>
      </div>

      {/* Stripe Payment Element */}
      <div className="rounded-lg border p-4">
        <PaymentElement
          options={{
            layout: "tabs",
          }}
        />
      </div>

      {/* Error message */}
      {errorMessage && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>{errorMessage}</AlertDescription>
        </Alert>
      )}

      {/* Action buttons */}
      <div className="flex gap-3">
        <Button
          type="button"
          variant="outline"
          onClick={onCancel}
          disabled={isProcessing}
          className="flex-1"
        >
          Anuluj
        </Button>
        <Button
          type="submit"
          disabled={!stripe || !elements || isProcessing}
          className="flex-1"
        >
          {isProcessing ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Przetwarzanie...
            </>
          ) : (
            <>
              <CreditCard className="mr-2 h-4 w-4" />
              Zapłać {formatCurrency(amount, currency)}
            </>
          )}
        </Button>
      </div>

      {/* Security notice */}
      <p className="text-center text-xs text-muted-foreground">
        Płatność jest bezpieczna i szyfrowana przez Stripe.
        Dane karty nie są przechowywane na naszych serwerach.
      </p>
    </form>
  );
}

function PaymentFormSkeleton() {
  return (
    <div className="space-y-6 animate-pulse">
      <div className="rounded-lg border bg-muted/50 p-4">
        <div className="flex items-center justify-between">
          <Skeleton className="h-4 w-20" />
          <Skeleton className="h-8 w-32" />
        </div>
      </div>
      <div className="rounded-lg border p-4">
        <div className="space-y-4">
          <Skeleton className="h-10 w-full" />
          <div className="grid grid-cols-2 gap-4">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </div>
        </div>
      </div>
      <div className="flex gap-3">
        <Skeleton className="h-10 flex-1" />
        <Skeleton className="h-10 flex-1" />
      </div>
    </div>
  );
}
