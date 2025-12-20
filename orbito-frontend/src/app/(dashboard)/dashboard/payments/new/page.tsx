import { ProcessPaymentForm } from "@/features/payments/components/ProcessPaymentForm";

export default function ProcessPaymentPage() {
  return (
    <div className="container mx-auto py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold">Process Manual Payment</h1>
        <p className="text-muted-foreground">
          Record a manual payment for a subscription
        </p>
      </div>

      <div className="max-w-2xl">
        <ProcessPaymentForm />
      </div>
    </div>
  );
}
