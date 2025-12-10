"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { UserPlus } from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/shared/ui/dialog";
import { Button } from "@/shared/ui/button";
import { Input } from "@/shared/ui/input";
import { Label } from "@/shared/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui/select";
import { Textarea } from "@/shared/ui/textarea";
import { TeamMemberRole } from "@/core/api/generated/models";
import { useInviteTeamMember } from "../hooks/useTeam";
import {
  InviteTeamMemberSchema,
  type InviteTeamMemberInput,
} from "../schemas/team.schemas";
import { getTeamMemberRoleName } from "../types/team.types";

export function TeamInviteForm() {
  const [open, setOpen] = useState(false);
  const inviteTeamMember = useInviteTeamMember();

  const {
    register,
    handleSubmit,
    setValue,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<InviteTeamMemberInput>({
    resolver: zodResolver(InviteTeamMemberSchema),
    defaultValues: {
      email: "",
      role: TeamMemberRole.NUMBER_3, // Default to Member
      firstName: "",
      lastName: "",
      message: "",
    },
  });

  const [selectedRole, setSelectedRole] = useState<TeamMemberRole>(
    TeamMemberRole.NUMBER_3
  );

  const onSubmit = async (data: InviteTeamMemberInput) => {
    try {
      await inviteTeamMember.mutateAsync({
        data: {
          email: data.email,
          role: data.role,
          firstName: data.firstName || null,
          lastName: data.lastName || null,
          message: data.message || null,
        },
      });
      reset();
      setOpen(false);
    } catch (error) {
      // Error is handled by the hook
      console.error("Failed to invite team member:", error);
    }
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button>
          <UserPlus className="mr-2 h-4 w-4" />
          Invite Team Member
        </Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Invite Team Member</DialogTitle>
          <DialogDescription>
            Send an invitation to a new team member. They will receive an email
            with a link to join your organization.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="email">
              Email <span className="text-destructive">*</span>
            </Label>
            <Input
              id="email"
              type="email"
              placeholder="colleague@example.com"
              {...register("email")}
            />
            {errors.email && (
              <p className="text-sm text-destructive">{errors.email.message}</p>
            )}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="firstName">First Name</Label>
              <Input
                id="firstName"
                placeholder="John"
                {...register("firstName")}
              />
              {errors.firstName && (
                <p className="text-sm text-destructive">
                  {errors.firstName.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="lastName">Last Name</Label>
              <Input id="lastName" placeholder="Doe" {...register("lastName")} />
              {errors.lastName && (
                <p className="text-sm text-destructive">
                  {errors.lastName.message}
                </p>
              )}
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="role">
              Role <span className="text-destructive">*</span>
            </Label>
            <Select
              value={selectedRole?.toString()}
              onValueChange={(value) => {
                const role = parseInt(value) as TeamMemberRole;
                setValue("role", role);
                setSelectedRole(role);
              }}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select a role" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value={TeamMemberRole.NUMBER_1.toString()}>
                  {getTeamMemberRoleName(TeamMemberRole.NUMBER_1)}
                </SelectItem>
                <SelectItem value={TeamMemberRole.NUMBER_2.toString()}>
                  {getTeamMemberRoleName(TeamMemberRole.NUMBER_2)}
                </SelectItem>
                <SelectItem value={TeamMemberRole.NUMBER_3.toString()}>
                  {getTeamMemberRoleName(TeamMemberRole.NUMBER_3)}
                </SelectItem>
              </SelectContent>
            </Select>
            {errors.role && (
              <p className="text-sm text-destructive">{errors.role.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="message">Personal Message (Optional)</Label>
            <Textarea
              id="message"
              placeholder="Welcome to the team! We're excited to have you..."
              rows={3}
              {...register("message")}
            />
            {errors.message && (
              <p className="text-sm text-destructive">{errors.message.message}</p>
            )}
          </div>

          <div className="flex justify-end gap-2">
            <Button
              type="button"
              variant="outline"
              onClick={() => setOpen(false)}
              disabled={isSubmitting}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Sending..." : "Send Invitation"}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
