import {
  useGetApiTeamMembersInvitations,
  usePostApiTeamMembersInvite,
} from "@/core/api/generated/team-members/team-members";
import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useInvitations() {
  const queryClient = useQueryClient();

  const { data, isLoading, error } = useGetApiTeamMembersInvitations();

  const inviteMutation = usePostApiTeamMembersInvite({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["/api/TeamMembers/invitations"] });
        queryClient.invalidateQueries({ queryKey: ["/api/TeamMembers"] });
        toast.success("Invitation sent!");
      },
      onError: (error: Error) => {
        toast.error(error.message || "Failed to send invitation");
      },
    },
  });

  return {
    invitations: data ?? [],
    isLoading,
    error,
    sendInvite: inviteMutation.mutate,
    isSending: inviteMutation.isPending,
  };
}
