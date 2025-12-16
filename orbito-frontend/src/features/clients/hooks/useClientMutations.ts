import {
  usePostApiClients,
  usePutApiClientsId,
  useDeleteApiClientsId,
} from "@/core/api/generated/clients/clients";
import type {
  CreateClientCommand,
  UpdateClientCommand,
} from "@/core/api/generated/models";
import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

export function useClientMutations() {
  const queryClient = useQueryClient();
  const router = useRouter();

  // Create mutation
  const createMutation = usePostApiClients({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["/api/Clients"] });
        toast.success("Client created successfully");
        router.push("/dashboard/clients");
      },
      onError: (error: unknown) => {
        const message = error instanceof Error ? error.message : "Failed to create client";
        toast.error(message);
      },
    },
  });

  // Update mutation
  const updateMutation = usePutApiClientsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["/api/Clients"] });
        toast.success("Client updated successfully");
        router.push("/dashboard/clients");
      },
      onError: (error: unknown) => {
        const message = error instanceof Error ? error.message : "Failed to update client";
        toast.error(message);
      },
    },
  });

  // Delete mutation
  const deleteMutation = useDeleteApiClientsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["/api/Clients"] });
        toast.success("Client deleted successfully");
        router.push("/dashboard/clients");
      },
      onError: (error: unknown) => {
        const message = error instanceof Error ? error.message : "Failed to delete client";
        toast.error(message);
      },
    },
  });

  return {
    createClient: (data: CreateClientCommand) =>
      createMutation.mutate({ data }),
    updateClient: (id: string, data: UpdateClientCommand) =>
      updateMutation.mutate({ id, data }),
    deleteClient: (id: string) => deleteMutation.mutate({ id }),
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}
