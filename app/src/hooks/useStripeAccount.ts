import {
  usePayoutAccountStatusQuery,
  useStripeOnboardingQuery,
} from "@/hooks/query/useStripeAccountQuery";

export function useStripeAccount() {
  const { data: accountStatus, isLoading } = usePayoutAccountStatusQuery(true);
  const { refetch, isFetching: isLoadingLink } = useStripeOnboardingQuery();

  return {
    isVerified: accountStatus === "Verified",
    isLoading,
    isLoadingLink,
    beginOnboarding: refetch,
  };
}
