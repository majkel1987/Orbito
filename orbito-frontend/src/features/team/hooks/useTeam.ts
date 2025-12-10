import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  useGetApiTeamMembers,
  useDeleteApiTeamMembersId,
  usePostApiTeamMembersInvite,
  usePutApiTeamMembersIdRole,
  getGetApiTeamMembersQueryKey,
} from "@/core/api/generated/team-members/team-members";
import type { TeamMemberDto } from "../types/team.types";

/**
 * Hook to fetch team members list
 * Wraps Orval hook with type assertion due to backend OpenAPI issue
 */
export function useTeamMembers() {
  const result = useGetApiTeamMembers() as {
    data: TeamMemberDto[] | undefined;
    isLoading: boolean;
    error: Error | null;
    refetch: () => void;
  };

  return result;
}

/**
 * Hook to invite a new team member
 */
export function useInviteTeamMember() {
  const queryClient = useQueryClient();

  return usePostApiTeamMembersInvite({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiTeamMembersQueryKey(),
        });
        toast.success("Team member invited successfully!");
      },
      onError: (error: Error) => {
        toast.error(error.message || "Failed to invite team member");
      },
    },
  });
}

/**
 * Hook to remove a team member
 */
export function useRemoveTeamMember() {
  const queryClient = useQueryClient();

  return useDeleteApiTeamMembersId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiTeamMembersQueryKey(),
        });
        toast.success("Team member removed successfully!");
      },
      onError: (error: Error) => {
        toast.error(error.message || "Failed to remove team member");
      },
    },
  });
}

/**
 * Hook to update team member role
 */
export function useUpdateTeamMemberRole() {
  const queryClient = useQueryClient();

  return usePutApiTeamMembersIdRole({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiTeamMembersQueryKey(),
        });
        toast.success("Team member role updated successfully!");
      },
      onError: (error: Error) => {
        toast.error(error.message || "Failed to update role");
      },
    },
  });
}
