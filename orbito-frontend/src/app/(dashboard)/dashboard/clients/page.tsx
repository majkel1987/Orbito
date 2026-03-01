import { ClientsTable } from "@/features/clients/components/ClientsTable";
import { InviteClientDialog } from "@/features/clients/components/InviteClientDialog";

export default function ClientsPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Clients</h1>
          <p className="text-muted-foreground">
            Manage your clients and their subscriptions
          </p>
        </div>
        <InviteClientDialog />
      </div>

      <ClientsTable />
    </div>
  );
}
