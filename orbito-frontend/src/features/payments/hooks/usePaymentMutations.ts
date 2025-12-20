"use client";

import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import {
  usePostApiPaymentProcess,
  usePostApiPaymentIdRefund,
} from "@/core/api/generated/payment/payment";
import type {
  ProcessPaymentInput,
  RefundPaymentInput,
} from "../schemas/payment.schemas";

/**
 * Hook for processing manual payment
 * POST /api/Payment/process
 */
export function useProcessPayment() {
  const queryClient = useQueryClient();
  const router = useRouter();

  return usePostApiPaymentProcess({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["api", "Payment"] });
        toast.success("Payment processed successfully!");
        router.push("/dashboard/payments");
      },
      onError: (error) => {
        const message = error instanceof Error ? error.message : "Unknown error";
        toast.error(`Failed to process payment: ${message}`);
      },
    },
  });
}

/**
 * Hook for refunding a payment
 * POST /api/Payment/{id}/refund
 */
export function useRefundPayment() {
  const queryClient = useQueryClient();

  return usePostApiPaymentIdRefund({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["api", "Payment"] });
        toast.success("Payment refunded successfully!");
      },
      onError: (error) => {
        const message = error instanceof Error ? error.message : "Unknown error";
        toast.error(`Failed to refund payment: ${message}`);
      },
    },
  });
}
