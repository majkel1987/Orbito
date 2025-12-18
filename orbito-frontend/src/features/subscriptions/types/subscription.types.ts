/**
 * Subscription DTO - matches backend structure
 * Based on: Orbito.Application.DTOs.SubscriptionDto
 */
export interface SubscriptionDto {
  id: string;
  tenantId: string;
  clientId: string;
  planId: string;
  status: string; // "Active", "Cancelled", "Expired", "Suspended", "Trial"
  amount: number;
  currency: string;
  billingPeriodValue: number;
  billingPeriodType: string; // "1 Monthly", "3 Yearly", etc.
  startDate: string;
  endDate: string | null;
  nextBillingDate: string;
  isInTrial: boolean;
  trialEndDate: string | null;
  externalSubscriptionId: string | null;
  createdAt: string;
  cancelledAt: string | null;
  updatedAt: string | null;

  // Optional details (when IncludeDetails is true)
  clientCompanyName?: string | null;
  clientEmail?: string | null;
  clientFirstName?: string | null;
  clientLastName?: string | null;
  planName?: string | null;
  planDescription?: string | null;
  paymentCount?: number;
  totalPaid?: number;
  lastPaymentDate?: string | null;
}

/**
 * Paginated list of subscriptions
 */
export interface SubscriptionDtoPaginatedList {
  items: SubscriptionDto[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/**
 * Helper: Get subscription status badge variant
 */
export function getSubscriptionStatusVariant(
  status: string
): "default" | "secondary" | "destructive" | "outline" {
  switch (status.toLowerCase()) {
    case "active":
      return "default";
    case "trial":
      return "secondary";
    case "cancelled":
    case "expired":
      return "destructive";
    case "suspended":
      return "secondary";
    default:
      return "outline";
  }
}

/**
 * Helper: Get client display name
 */
export function getClientDisplayName(subscription: SubscriptionDto): string {
  if (subscription.clientCompanyName) {
    return subscription.clientCompanyName;
  }
  if (subscription.clientFirstName || subscription.clientLastName) {
    return `${subscription.clientFirstName || ""} ${subscription.clientLastName || ""}`.trim();
  }
  return subscription.clientEmail || "Unknown Client";
}
