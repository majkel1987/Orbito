import { TeamTable } from "@/features/team/components/TeamTable";
import { TeamInviteForm } from "@/features/team/components/TeamInviteForm";

export default function TeamPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Team</h1>
          <p className="text-muted-foreground">
            Manage your organization&apos;s team members and their roles.
          </p>
        </div>
        <TeamInviteForm />
      </div>

      <TeamTable />
    </div>
  );
}
