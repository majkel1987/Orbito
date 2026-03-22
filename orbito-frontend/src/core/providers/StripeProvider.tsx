"use client";

import { Elements } from "@stripe/react-stripe-js";
import { loadStripe, type Stripe } from "@stripe/stripe-js";
import { useEffect, useState } from "react";

// Lazy-load Stripe to improve initial page load
let stripePromise: Promise<Stripe | null> | null = null;

function getStripe() {
  if (!stripePromise) {
    const publishableKey = process.env.NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY;
    if (!publishableKey) {
      console.error("NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY is not set");
      return null;
    }
    stripePromise = loadStripe(publishableKey);
  }
  return stripePromise;
}

interface StripeProviderProps {
  children: React.ReactNode;
  clientSecret?: string;
}

/**
 * StripeProvider wraps children with Stripe Elements context.
 *
 * Usage:
 * 1. Without clientSecret - loads Stripe but doesn't initialize Elements
 * 2. With clientSecret - initializes Elements for PaymentIntent confirmation
 */
export function StripeProvider({ children, clientSecret }: StripeProviderProps) {
  const [stripe, setStripe] = useState<Stripe | null>(null);

  useEffect(() => {
    const stripeInstance = getStripe();
    if (stripeInstance) {
      stripeInstance.then(setStripe);
    }
  }, []);

  // If no Stripe key is configured, just render children
  if (!process.env.NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY) {
    return <>{children}</>;
  }

  // If clientSecret is provided, use Elements with appearance
  if (clientSecret && stripe) {
    return (
      <Elements
        stripe={stripe}
        options={{
          clientSecret,
          appearance: {
            theme: "stripe",
            variables: {
              colorPrimary: "#0f172a",
              colorBackground: "#ffffff",
              colorText: "#1e293b",
              colorDanger: "#ef4444",
              fontFamily: "Inter, system-ui, sans-serif",
              borderRadius: "8px",
            },
          },
          locale: "pl",
        }}
      >
        {children}
      </Elements>
    );
  }

  // Without clientSecret, just render children (Stripe loaded but not Elements)
  return <>{children}</>;
}

/**
 * Hook to check if Stripe is properly configured
 */
export function useStripeConfigured(): boolean {
  return !!process.env.NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY;
}
