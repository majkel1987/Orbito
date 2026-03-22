"use client";

import { useMemo } from "react";
import { useSearchParams, redirect } from "next/navigation";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/ui/card";
import { Button } from "@/shared/ui/button";
import { CheckCircle, XCircle, ArrowLeft, Clock } from "lucide-react";
import Link from "next/link";

type PaymentStatus = "success" | "processing" | "error";

/**
 * Payment success page - Stripe redirects here after payment.
 * URL params from Stripe:
 * - payment_intent: PaymentIntent ID
 * - payment_intent_client_secret: client secret
 * - redirect_status: "succeeded" | "processing" | "requires_payment_method"
 */
export default function PaymentSuccessPage() {
  const searchParams = useSearchParams();

  // Derive status from URL params directly, no state needed
  const status = useMemo((): PaymentStatus => {
    const redirectStatus = searchParams.get("redirect_status");
    const paymentIntent = searchParams.get("payment_intent");

    if (!paymentIntent) {
      // No payment intent - will redirect
      return "error";
    }

    switch (redirectStatus) {
      case "succeeded":
        return "success";
      case "processing":
        return "processing";
      default:
        return "error";
    }
  }, [searchParams]);

  // Redirect if no payment intent
  if (!searchParams.get("payment_intent")) {
    redirect("/portal");
  }

  return (
    <div className="flex items-center justify-center min-h-[60vh]">
      <Card className="w-full max-w-md">
        {status === "success" && (
          <>
            <CardHeader className="text-center">
              <div className="mx-auto mb-4">
                <CheckCircle className="h-12 w-12 text-green-500" />
              </div>
              <CardTitle className="text-green-700">Płatność zakończona pomyślnie</CardTitle>
              <CardDescription>
                Dziękujemy za dokonanie płatności. Twoja subskrypcja została opłacona.
              </CardDescription>
            </CardHeader>
            <CardContent className="flex justify-center">
              <Button asChild>
                <Link href="/portal">
                  <ArrowLeft className="mr-2 h-4 w-4" />
                  Wróć do portalu
                </Link>
              </Button>
            </CardContent>
          </>
        )}

        {status === "processing" && (
          <>
            <CardHeader className="text-center">
              <div className="mx-auto mb-4">
                <Clock className="h-12 w-12 text-amber-500" />
              </div>
              <CardTitle className="text-amber-700">Płatność w trakcie przetwarzania</CardTitle>
              <CardDescription>
                Twoja płatność jest przetwarzana. Otrzymasz potwierdzenie, gdy zostanie zakończona.
              </CardDescription>
            </CardHeader>
            <CardContent className="flex justify-center">
              <Button asChild variant="outline">
                <Link href="/portal">
                  <ArrowLeft className="mr-2 h-4 w-4" />
                  Wróć do portalu
                </Link>
              </Button>
            </CardContent>
          </>
        )}

        {status === "error" && (
          <>
            <CardHeader className="text-center">
              <div className="mx-auto mb-4">
                <XCircle className="h-12 w-12 text-red-500" />
              </div>
              <CardTitle className="text-red-700">Płatność nieudana</CardTitle>
              <CardDescription>
                Nie udało się przetworzyć płatności. Spróbuj ponownie lub użyj innej metody płatności.
              </CardDescription>
            </CardHeader>
            <CardContent className="flex justify-center">
              <Button asChild variant="outline">
                <Link href="/portal">
                  <ArrowLeft className="mr-2 h-4 w-4" />
                  Wróć do portalu
                </Link>
              </Button>
            </CardContent>
          </>
        )}
      </Card>
    </div>
  );
}
