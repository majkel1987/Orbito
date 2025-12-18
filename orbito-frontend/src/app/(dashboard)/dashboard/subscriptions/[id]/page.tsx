import Link from "next/link";
import { notFound } from "next/navigation";
import { SubscriptionDetail } from "@/features/subscriptions/components/SubscriptionDetail";
import { Button } from "@/shared/ui/button";

interface SubscriptionDetailPageProps {
  params: Promise<{ id: string }>;
}

export default async function SubscriptionDetailPage({
  params,
}: SubscriptionDetailPageProps) {
  const { id } = await params;

  if (!id) {
    notFound();
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Subscription Details</h1>
          <p className="text-muted-foreground">View and manage subscription</p>
        </div>
        <Button variant="outline" asChild>
          <Link href="/dashboard/subscriptions">Back to Subscriptions</Link>
        </Button>
      </div>

      <SubscriptionDetail subscriptionId={id} />
    </div>
  );
}
