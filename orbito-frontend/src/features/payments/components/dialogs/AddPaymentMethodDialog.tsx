"use client";

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/shared/ui/dialog";
import { Button } from "@/shared/ui/button";
import { CreditCard, Shield } from "lucide-react";

interface AddPaymentMethodDialogProps {
  isOpen: boolean;
  onClose: () => void;
  clientId: string;
}

/**
 * AddPaymentMethodDialog - PCI DSS Compliant
 *
 * IMPORTANT: This dialog does NOT collect card details directly.
 * Instead, it informs the user they will be redirected to Stripe's secure payment page.
 * This approach ensures PCI DSS compliance by not handling sensitive card data.
 */
export function AddPaymentMethodDialog({
  isOpen,
  onClose,
  clientId,
}: AddPaymentMethodDialogProps) {
  const handleProceedToStripe = () => {
    // TODO: In production, this would:
    // 1. Create a Stripe Customer (if not exists)
    // 2. Create a Stripe Setup Intent
    // 3. Redirect to Stripe Checkout or use Stripe Elements

    console.log("Proceeding to Stripe for client:", clientId);

    // For now, show a placeholder message
    alert(
      "In production, you would be redirected to Stripe's secure payment page to add your card.\n\n" +
      "This ensures PCI DSS compliance by not storing sensitive card data on our servers."
    );

    onClose();
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <CreditCard className="h-5 w-5" />
            Add Payment Method
          </DialogTitle>
          <DialogDescription>
            Securely add a payment method for future transactions
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          {/* Security Notice */}
          <div className="rounded-lg border border-blue-200 bg-blue-50 p-4">
            <div className="flex gap-3">
              <Shield className="h-5 w-5 text-blue-600 shrink-0 mt-0.5" />
              <div>
                <p className="font-medium text-blue-900">Secure Payment Processing</p>
                <p className="mt-1 text-sm text-blue-700">
                  You will be redirected to our secure payment provider (Stripe) to add your card details.
                  We never store your full card information on our servers for PCI DSS compliance.
                </p>
              </div>
            </div>
          </div>

          {/* Stripe Elements Placeholder */}
          <div className="rounded-lg border border-dashed border-gray-300 bg-gray-50 p-8 text-center">
            <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-blue-100">
              <CreditCard className="h-6 w-6 text-blue-600" />
            </div>
            <p className="mt-4 text-sm font-medium text-gray-900">
              Stripe Secure Checkout
            </p>
            <p className="mt-1 text-xs text-gray-500">
              Card details are handled by Stripe Elements
            </p>
          </div>

          {/* Actions */}
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onClose}>
              Cancel
            </Button>
            <Button onClick={handleProceedToStripe}>
              Proceed to Secure Checkout
            </Button>
          </div>

          {/* Info Text */}
          <p className="text-xs text-gray-500 text-center">
            By adding a payment method, you agree to our terms of service.
            Your payment information is encrypted and secure.
          </p>
        </div>
      </DialogContent>
    </Dialog>
  );
}
