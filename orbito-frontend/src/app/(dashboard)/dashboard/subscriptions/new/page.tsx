import Link from "next/link";
import { CreateSubscriptionWizard } from "@/features/subscriptions/components/wizard/CreateSubscriptionWizard";
import { Button } from "@/shared/ui/button";

export default function NewSubscriptionPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Create Subscription</h1>
          <p className="text-muted-foreground">
            Create a new subscription for a client
          </p>
        </div>
        <Button variant="outline" asChild>
          <Link href="/dashboard/subscriptions">Cancel</Link>
        </Button>
      </div>

      <CreateSubscriptionWizard />
    </div>
  );
}
