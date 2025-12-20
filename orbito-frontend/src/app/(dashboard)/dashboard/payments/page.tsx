import Link from "next/link";
import { Button } from "@/shared/ui/button";
import { PaymentsTable } from "@/features/payments/components/PaymentsTable";

export default function PaymentsPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Payment History</h1>
          <p className="text-gray-500 mt-2">
            View and manage all payment transactions
          </p>
        </div>
        <Link href="/dashboard/payments/new">
          <Button>Process Payment</Button>
        </Link>
      </div>

      <PaymentsTable />
    </div>
  );
}
