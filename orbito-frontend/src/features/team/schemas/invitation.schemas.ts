import { z } from "zod";
import { TeamMemberRole } from "@/core/api/generated/models";

export const InviteSchema = z.object({
  email: z.string().email("Invalid email address"),
  role: z.union([
    z.literal(TeamMemberRole.NUMBER_1), // Admin
    z.literal(TeamMemberRole.NUMBER_3), // Member
  ]),
});

export type InviteInput = z.infer<typeof InviteSchema>;
