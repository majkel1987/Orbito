"use client";

import { PaymentElement, useStripe, useElements } from "@stripe/react-stripe-js";
import { useState } from "react";
import { Button } from "@/shared/ui/button";
import { Alert, AlertDescription } from "@/shared/ui/alert";
import { Loader2, CreditCard, AlertCircle } from "lucide-react";
import { formatCurrency } from "@/shared/lib/formatters";

interface PaymentFormProps {
  amount: number;
  currency: string;
  onCancel: () => void;
}

/**
 * PaymentForm uses Stripe Payment Element for PCI DSS compliant card collection.
 * Card data NEVER touches our servers - goes directly to Stripe.
 * On success, Stripe redirects to /portal/payment-success page.
 */
export function PaymentForm({
  amount,
  currency,
  onCancel,
}: PaymentFormProps) {
  const stripe = useStripe();
  const elements = useElements();
  const [isProcessing, setIsProcessing] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!stripe || !elements) {
      // Stripe.js hasn't loaded yet
      return;
    }

    setIsProcessing(true);
    setErrorMessage(null);

    try {
      const { error } = await stripe.confirmPayment({
        elements,
        confirmParams: {
          return_url: `${window.location.origin}/portal/payment-success`,
        },
      });

      // This will only be reached if there's an immediate error
      // (card declined, validation error, etc.)
      // If successful, user is redirected to return_url
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

      {/* Stripe Payment Element - card input */}
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

/**
 * Loading state for PaymentForm
 */
export function PaymentFormSkeleton() {
  return (
    <div className="space-y-6 animate-pulse">
      <div className="rounded-lg border bg-muted/50 p-4">
        <div className="flex items-center justify-between">
          <div className="h-4 w-20 rounded bg-muted" />
          <div className="h-8 w-32 rounded bg-muted" />
        </div>
      </div>
      <div className="rounded-lg border p-4">
        <div className="space-y-4">
          <div className="h-10 rounded bg-muted" />
          <div className="grid grid-cols-2 gap-4">
            <div className="h-10 rounded bg-muted" />
            <div className="h-10 rounded bg-muted" />
          </div>
        </div>
      </div>
      <div className="flex gap-3">
        <div className="h-10 flex-1 rounded bg-muted" />
        <div className="h-10 flex-1 rounded bg-muted" />
      </div>
    </div>
  );
}
