import { PaymentDetail } from "@/features/payments/components/PaymentDetail";

interface PaymentDetailPageProps {
  params: Promise<{ id: string }>;
}

export default async function PaymentDetailPage({
  params,
}: PaymentDetailPageProps) {
  const { id } = await params;

  return (
    <div className="container mx-auto py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold">Payment Details</h1>
        <p className="text-muted-foreground">
          View payment information and perform actions
        </p>
      </div>

      <PaymentDetail paymentId={id} />
    </div>
  );
}
