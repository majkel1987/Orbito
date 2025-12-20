"use client";

import { useState } from "react";
import { Button } from "@/shared/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { Badge } from "@/shared/ui/badge";
import { Skeleton } from "@/shared/ui/skeleton";
import { CreditCard, Plus, Trash2 } from "lucide-react";
import { useGetApiPaymentPaymentMethodsClientClientId } from "@/core/api/generated/payment/payment";
import { AddPaymentMethodDialog } from "./dialogs/AddPaymentMethodDialog";

interface PaymentMethodListProps {
  clientId: string;
}

/**
 * PaymentMethodList Component
 *
 * Displays a list of payment methods for a specific client.
 * Shows masked card numbers (e.g., "**** **** **** 4242") and brand.
 *
 * IMPORTANT: This component only displays payment methods fetched from the API.
 * It does NOT handle actual card data - that's managed by Stripe.
 */
export function PaymentMethodList({ clientId }: PaymentMethodListProps) {
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);

  const { data, isLoading, error } = useGetApiPaymentPaymentMethodsClientClientId(
    clientId,
    {
      pageNumber: 1,
      pageSize: 10,
      activeOnly: true,
    }
  );

  // Loading state
  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <CreditCard className="h-5 w-5" />
            Payment Methods
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <Skeleton className="h-16 w-full" />
          <Skeleton className="h-16 w-full" />
        </CardContent>
      </Card>
    );
  }

  // Error state
  if (error) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <CreditCard className="h-5 w-5" />
            Payment Methods
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-red-600">
            Error loading payment methods:{" "}
            {error instanceof Error ? error.message : "Unknown error"}
          </div>
        </CardContent>
      </Card>
    );
  }

  const paymentMethods = data?.paymentMethods || [];

  return (
    <>
      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0">
          <CardTitle className="flex items-center gap-2">
            <CreditCard className="h-5 w-5" />
            Payment Methods
          </CardTitle>
          <Button
            size="sm"
            onClick={() => setIsAddDialogOpen(true)}
            className="gap-2"
          >
            <Plus className="h-4 w-4" />
            Add Card
          </Button>
        </CardHeader>
        <CardContent>
          {/* Empty state */}
          {paymentMethods.length === 0 && (
            <div className="rounded-lg border border-dashed border-gray-300 bg-gray-50 p-8 text-center">
              <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-gray-100">
                <CreditCard className="h-6 w-6 text-gray-400" />
              </div>
              <p className="mt-4 text-sm font-medium text-gray-900">
                No payment methods
              </p>
              <p className="mt-1 text-sm text-gray-500">
                Add a card to enable automatic payments
              </p>
              <Button
                onClick={() => setIsAddDialogOpen(true)}
                className="mt-4"
                variant="outline"
              >
                Add Your First Card
              </Button>
            </div>
          )}

          {/* Payment methods list */}
          {paymentMethods.length > 0 && (
            <div className="space-y-3">
              {paymentMethods.map((method) => (
                <div
                  key={method.id}
                  className="flex items-center justify-between rounded-lg border p-4 hover:bg-gray-50"
                >
                  <div className="flex items-center gap-3">
                    <div className="flex h-10 w-10 items-center justify-center rounded-full bg-blue-100">
                      <CreditCard className="h-5 w-5 text-blue-600" />
                    </div>
                    <div>
                      <div className="flex items-center gap-2">
                        <span className="font-medium">
                          {method.type || "Card"}
                        </span>
                        <span className="font-mono text-sm text-gray-600">
                          •••• {method.lastFourDigits || "****"}
                        </span>
                      </div>
                      {method.expiryDate && (
                        <p className="text-xs text-gray-500">
                          Expires {method.expiryDate}
                        </p>
                      )}
                      {method.isExpired && (
                        <p className="text-xs text-red-500 font-medium">
                          Expired
                        </p>
                      )}
                    </div>
                  </div>

                  <div className="flex items-center gap-2">
                    {method.isDefault && (
                      <Badge variant="default">Default</Badge>
                    )}
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8 text-red-500 hover:text-red-700"
                      onClick={() => {
                        // TODO: Implement delete payment method
                        console.log("Delete payment method:", method.id);
                      }}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          )}

          {/* Info text */}
          <p className="mt-4 text-xs text-gray-500 text-center">
            Payment information is securely stored by our payment provider
            (Stripe). We never store your full card details.
          </p>
        </CardContent>
      </Card>

      {/* Add Payment Method Dialog */}
      <AddPaymentMethodDialog
        isOpen={isAddDialogOpen}
        onClose={() => setIsAddDialogOpen(false)}
        clientId={clientId}
      />
    </>
  );
}
