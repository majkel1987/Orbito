import { useGetApiSubscriptionPlans } from "@/core/api/generated/subscription-plans/subscription-plans";
import type { SubscriptionPlanDto } from "../types/plan.types";

export function usePlans() {
  const { data, isLoading, error } = useGetApiSubscriptionPlans() as {
    data: unknown;
    isLoading: boolean;
    error: Error | null;
  };

  // Backend zwraca Result<T> który jest unwrapowany przez interceptor do { items: [...] }
  let plans: SubscriptionPlanDto[] = [];

  if (Array.isArray(data)) {
    // Jeśli backend zwróci bezpośrednią tablicę
    plans = data as SubscriptionPlanDto[];
  } else if (data && typeof data === "object") {
    // Backend zwraca obiekt z kluczem (np. { items: [...] })
    const obj = data as Record<string, unknown>;
    if (Array.isArray(obj.items)) {
      plans = obj.items as SubscriptionPlanDto[];
    } else if (Array.isArray(obj.subscriptionPlans)) {
      plans = obj.subscriptionPlans as SubscriptionPlanDto[];
    } else if (Array.isArray(obj.plans)) {
      plans = obj.plans as SubscriptionPlanDto[];
    }
  }

  return {
    plans,
    isLoading,
    error,
  };
}
