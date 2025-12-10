import type { TeamMemberRole } from "@/core/api/generated/models";

/**
 * Team member DTO returned from API
 * Note: Backend doesn't export this type in Swagger, so we define it manually
 */
export interface TeamMemberDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: TeamMemberRole;
  status: TeamMemberStatus;
  invitedAt?: string;
  joinedAt?: string;
  lastActiveAt?: string;
}

/**
 * Team member status enum
 */
export enum TeamMemberStatus {
  Active = "Active",
  Invited = "Invited",
  Deactivated = "Deactivated",
}

/**
 * Helper to get human-readable role name
 */
export function getTeamMemberRoleName(role: TeamMemberRole): string {
  switch (role) {
    case 1:
      return "Admin";
    case 2:
      return "Manager";
    case 3:
      return "Member";
    default:
      return "Unknown";
  }
}

/**
 * Helper to get role badge variant
 */
export function getTeamMemberRoleVariant(
  role: TeamMemberRole
): "default" | "secondary" | "outline" {
  switch (role) {
    case 1:
      return "default"; // Admin
    case 2:
      return "secondary"; // Manager
    case 3:
      return "outline"; // Member
    default:
      return "outline";
  }
}
