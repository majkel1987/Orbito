import { PaymentDetail } from "@/features/payments/components/PaymentDetail";

interface PaymentDetailPageProps {
  params: Promise<{ id: string }>;
  searchParams: Promise<{ clientId?: string }>;
}

export default async function PaymentDetailPage({
  params,
  searchParams,
}: PaymentDetailPageProps) {
  const { id } = await params;
  const { clientId } = await searchParams;

  return (
    <div className="container mx-auto py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold">Payment Details</h1>
        <p className="text-muted-foreground">
          View payment information and perform actions
        </p>
      </div>

      <PaymentDetail paymentId={id} clientId={clientId} />
    </div>
  );
}
