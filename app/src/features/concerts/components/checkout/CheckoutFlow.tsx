import type { ReactNode } from "react";
import {
  CheckoutAwaiting,
  type AwaitingStep,
} from "./CheckoutAwaiting";
import type { CheckoutFlowState } from "../../hooks/useCheckoutFlow";

interface PhaseSteps {
  first: string;
  final: string;
}

interface Props<TResult> {
  flow: CheckoutFlowState<TResult>;
  title: string;
  timeoutTitle: string;
  pendingHint: string;
  steps: PhaseSteps;
  renderSuccess: (result: TResult) => ReactNode;
}

export function CheckoutFlow<TResult>({
  flow,
  title,
  timeoutTitle,
  pendingHint,
  steps,
  renderSuccess,
}: Props<TResult>) {
  if (flow.phase === "success") return <>{renderSuccess(flow.result)}</>;

  const stepList: AwaitingStep[] = [
    { label: steps.first, status: "done" },
    { label: "Confirming with our system", status: "active" },
    { label: steps.final, status: "pending" },
  ];

  const awaiting =
    flow.phase === "awaiting"
      ? {
          title,
          description:
            "This usually takes a few seconds. Please don't close this page.",
        }
      : {
          title: timeoutTitle,
          description: `This is taking longer than usual. ${pendingHint} shortly. Feel free to keep using the app.`,
        };
  return <CheckoutAwaiting {...awaiting} steps={stepList} />;
}
