import Link from "next/link";
import { Button } from "@/shared/ui/button";
import { Plus } from "lucide-react";
import { SubscriptionsTable } from "@/features/subscriptions/components/SubscriptionsTable";

export default function SubscriptionsPage() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Subscriptions</h1>
          <p className="text-gray-500">
            Manage client subscriptions and billing
          </p>
        </div>
        <Button asChild>
          <Link href="/dashboard/subscriptions/new">
            <Plus className="mr-2 h-4 w-4" />
            Create Subscription
          </Link>
        </Button>
      </div>

      {/* Subscriptions table */}
      <SubscriptionsTable />
    </div>
  );
}
