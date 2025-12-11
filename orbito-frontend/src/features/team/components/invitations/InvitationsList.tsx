"use client";

import { useInvitations } from "../../hooks/useInvitations";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/ui/table";
import { Badge } from "@/shared/ui/badge";
import { Skeleton } from "@/shared/ui/skeleton";
import { formatDate } from "@/shared/lib/formatters";
import { getTeamMemberRoleName } from "../../types/team.types";
import type { TeamMemberDto } from "../../types/team.types";

export function InvitationsList() {
  const { invitations, isLoading, error } = useInvitations();

  if (error) {
    return (
      <div className="text-red-500">
        Error: {error instanceof Error ? error.message : "Unknown error"}
      </div>
    );
  }

  if (isLoading) {
    return <InvitationsListSkeleton />;
  }

  // Type assertion needed because backend doesn't export proper types
  const typedInvitations = (invitations as unknown) as TeamMemberDto[];

  if (!typedInvitations || typedInvitations.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground">
        No pending invitations
      </div>
    );
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Email</TableHead>
          <TableHead>Role</TableHead>
          <TableHead>Sent</TableHead>
          <TableHead>Expires</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {typedInvitations.map((invitation) => (
          <TableRow key={invitation.id}>
            <TableCell>{invitation.email}</TableCell>
            <TableCell>
              <Badge variant="outline">
                {getTeamMemberRoleName(invitation.role)}
              </Badge>
            </TableCell>
            <TableCell>
              {invitation.invitedAt ? formatDate(invitation.invitedAt) : "N/A"}
            </TableCell>
            <TableCell>
              {invitation.invitedAt
                ? formatDate(
                    new Date(
                      new Date(invitation.invitedAt).getTime() +
                        7 * 24 * 60 * 60 * 1000
                    ).toISOString()
                  )
                : "N/A"}
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}

function InvitationsListSkeleton() {
  return (
    <div className="space-y-2">
      {[...Array(3)].map((_, i) => (
        <Skeleton key={i} className="h-12 w-full" />
      ))}
    </div>
  );
}
