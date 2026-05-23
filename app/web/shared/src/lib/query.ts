import { useQuery as useQueryBase } from "@tanstack/react-query";
import type { UseQueryOptions, UseQueryResult } from "@tanstack/react-query";

export function useQuery<TData, TError = Error>(
  id: number | undefined,
  options: Omit<UseQueryOptions<TData, TError>, "enabled">,
): UseQueryResult<TData, TError> {
  return useQueryBase({ ...options, enabled: !!id });
}
