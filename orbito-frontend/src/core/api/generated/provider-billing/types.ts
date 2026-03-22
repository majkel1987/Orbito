/**
 * Manual types for Provider Billing (pending Orval regeneration)
 */

export interface CreateProviderPaymentIntentCommand {
  platformPlanId?: string | null;
}

export interface CreateProviderPaymentIntentResponse {
  clientSecret: string;
  paymentIntentId: string;
  amount: number;
  currency: string;
  planName: string;
}
