import { usePostApiClientsInvite } from "@/core/api/generated/clients/clients";
import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useInviteClient() {
  const queryClient = useQueryClient();

  return usePostApiClientsInvite({
    mutation: {
      onSuccess: () => {
        toast.success("Zaproszenie zostało wysłane!");
        queryClient.invalidateQueries({ queryKey: ["/api/Clients"] });
      },
      onError: (error: unknown) => {
        const message =
          error instanceof Error ? error.message : "Nie udało się wysłać zaproszenia";
        toast.error(message);
      },
    },
  });
}
