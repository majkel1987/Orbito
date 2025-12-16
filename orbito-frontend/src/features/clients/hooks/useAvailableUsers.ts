import { useQuery } from "@tanstack/react-query";
import { customInstance } from "@/core/api/client";

export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
}

async function getAvailableUsers(): Promise<UserDto[]> {
  const response = await customInstance<UserDto[]>({
    url: "/api/Users/available-for-client",
    method: "GET",
  });
  return response;
}

interface UseAvailableUsersOptions {
  enabled?: boolean;
}

export function useAvailableUsers(options?: UseAvailableUsersOptions) {
  return useQuery({
    queryKey: ["/api/Users/available-for-client"],
    queryFn: getAvailableUsers,
    staleTime: 5 * 60 * 1000, // 5 minutes
    enabled: options?.enabled ?? true, // Default to true for backwards compatibility
  });
}
