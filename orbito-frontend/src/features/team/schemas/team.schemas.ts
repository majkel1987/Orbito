import { z } from "zod";
import { TeamMemberRole } from "@/core/api/generated/models";

/**
 * Schema for inviting a team member
 */
export const InviteTeamMemberSchema = z.object({
  email: z.string().email("Invalid email address"),
  role: z.nativeEnum(TeamMemberRole).refine((val) => val in TeamMemberRole, {
    message: "Please select a role",
  }),
  firstName: z.string().min(2, "First name must be at least 2 characters").optional(),
  lastName: z.string().min(2, "Last name must be at least 2 characters").optional(),
  message: z.string().max(500, "Message must not exceed 500 characters").optional(),
});

export type InviteTeamMemberInput = z.infer<typeof InviteTeamMemberSchema>;

/**
 * Schema for updating team member role
 */
export const UpdateTeamMemberRoleSchema = z.object({
  role: z.nativeEnum(TeamMemberRole).refine((val) => val in TeamMemberRole, {
    message: "Please select a role",
  }),
});

export type UpdateTeamMemberRoleInput = z.infer<typeof UpdateTeamMemberRoleSchema>;
