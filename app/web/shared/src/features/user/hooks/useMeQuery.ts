import { useAuth } from "react-oidc-context";
import {
  queryOptions,
  skipToken,
  useQuery,
  type UseQueryResult,
} from "@tanstack/react-query";
import type { User } from "@/features/auth/types";

export const meQueryKey = ["auth", "me"] as const;

export function meQueryOptions<TUser extends User>(
  getMe: () => Promise<TUser>,
) {
  return queryOptions({
    queryKey: meQueryKey,
    queryFn: getMe,
    meta: { expectedErrors: [404] },
  });
}

export function useMeQuery(): UseQueryResult<User>;
export function useMeQuery<TUser extends User>(
  getMe: () => Promise<TUser>,
): UseQueryResult<TUser>;
export function useMeQuery<TUser extends User>(
  getMe?: () => Promise<TUser>,
): UseQueryResult<TUser> {
  const { isAuthenticated, isLoading } = useAuth();

  return useQuery<TUser>({
    queryKey: meQueryKey,
    queryFn: getMe ?? skipToken,
    meta: { expectedErrors: [404] },
    enabled: getMe !== undefined && !isLoading && isAuthenticated,
  });
}
