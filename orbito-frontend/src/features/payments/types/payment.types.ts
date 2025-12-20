import { type VariantProps } from "class-variance-authority";
import { badgeVariants } from "@/shared/ui/badge";

// Re-export PaymentDto from generated models
export type { PaymentDto } from "@/core/api/generated/models/paymentDto";
export type { GetAllPaymentsResponse as PaymentDtoPaginatedList } from "@/core/api/generated/models/getAllPaymentsResponse";

// Helper function for payment status badge variant
export function getPaymentStatusVariant(
  status: string
): VariantProps<typeof badgeVariants>["variant"] {
  switch (status.toLowerCase()) {
    case "completed":
    case "success":
      return "default"; // green
    case "pending":
    case "processing":
      return "secondary"; // gray
    case "failed":
      return "destructive"; // red
    case "refunded":
      return "outline"; // outlined
    default:
      return "secondary";
  }
}

// Helper function for payment status label
export function getPaymentStatusLabel(status: string): string {
  switch (status.toLowerCase()) {
    case "completed":
      return "Completed";
    case "pending":
      return "Pending";
    case "processing":
      return "Processing";
    case "failed":
      return "Failed";
    case "refunded":
      return "Refunded";
    default:
      return status;
  }
}
