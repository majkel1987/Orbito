"use client";

import { useState } from "react";
import { MoreHorizontal, Trash2, UserCog } from "lucide-react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/ui/table";
import { Badge } from "@/shared/ui/badge";
import { Button } from "@/shared/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/shared/ui/dropdown-menu";
import { Skeleton } from "@/shared/ui/skeleton";
import { useTeamMembers, useRemoveTeamMember } from "../hooks/useTeam";
import {
  getTeamMemberRoleName,
  getTeamMemberRoleVariant,
} from "../types/team.types";
import { formatDate } from "@/shared/lib/formatters";

export function TeamTable() {
  const { data: members, isLoading, error } = useTeamMembers();
  const removeTeamMember = useRemoveTeamMember();
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to remove this team member?")) {
      return;
    }

    setDeletingId(id);
    try {
      await removeTeamMember.mutateAsync({ id });
    } finally {
      setDeletingId(null);
    }
  };

  if (isLoading) {
    return <TeamTableSkeleton />;
  }

  if (error) {
    return (
      <div className="rounded-md border border-red-200 bg-red-50 p-4">
        <p className="text-sm text-red-800">
          Error loading team members: {error.message}
        </p>
      </div>
    );
  }

  if (!members || members.length === 0) {
    return (
      <div className="rounded-md border border-border p-8 text-center">
        <p className="text-sm text-muted-foreground">
          No team members found. Invite your first team member to get started.
        </p>
      </div>
    );
  }

  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Name</TableHead>
            <TableHead>Email</TableHead>
            <TableHead>Role</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Joined</TableHead>
            <TableHead className="w-[70px]"></TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {members.map((member) => (
            <TableRow key={member.id}>
              <TableCell className="font-medium">
                {member.firstName} {member.lastName}
              </TableCell>
              <TableCell>{member.email}</TableCell>
              <TableCell>
                <Badge variant={getTeamMemberRoleVariant(member.role)}>
                  {getTeamMemberRoleName(member.role)}
                </Badge>
              </TableCell>
              <TableCell>
                <Badge
                  variant={
                    member.status === "Active" ? "default" : "outline"
                  }
                >
                  {member.status}
                </Badge>
              </TableCell>
              <TableCell className="text-sm text-muted-foreground">
                {member.joinedAt
                  ? formatDate(member.joinedAt)
                  : member.invitedAt
                    ? `Invited ${formatDate(member.invitedAt)}`
                    : "—"}
              </TableCell>
              <TableCell>
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="h-8 w-8 p-0"
                      disabled={deletingId === member.id}
                    >
                      <span className="sr-only">Open menu</span>
                      <MoreHorizontal className="h-4 w-4" />
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <DropdownMenuLabel>Actions</DropdownMenuLabel>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem>
                      <UserCog className="mr-2 h-4 w-4" />
                      Change Role
                    </DropdownMenuItem>
                    <DropdownMenuItem
                      className="text-destructive"
                      onClick={() => handleDelete(member.id)}
                    >
                      <Trash2 className="mr-2 h-4 w-4" />
                      Remove
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}

function TeamTableSkeleton() {
  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Name</TableHead>
            <TableHead>Email</TableHead>
            <TableHead>Role</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Joined</TableHead>
            <TableHead className="w-[70px]"></TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {[...Array(5)].map((_, i) => (
            <TableRow key={i}>
              <TableCell>
                <Skeleton className="h-4 w-32" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-40" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-5 w-16" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-5 w-16" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-24" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-8 w-8" />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
