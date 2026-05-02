import { motion } from "framer-motion";
import { Check, Loader2 } from "lucide-react";

export interface AwaitingStep {
  label: string;
  status: "done" | "active" | "pending";
}

interface Props {
  title: string;
  description: string;
  steps: AwaitingStep[];
}

export function CheckoutAwaiting({ title, description, steps }: Props) {
  return (
    <motion.div
      initial={{ opacity: 0, y: 8 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className="mx-auto flex min-h-[60vh] max-w-md flex-col items-center justify-center px-4 py-12 text-center"
    >
      <div className="bg-primary/10 mb-5 flex size-20 items-center justify-center rounded-full">
        <Loader2 className="text-primary size-10 animate-spin" />
      </div>
      <h1 className="mb-3 text-2xl font-bold tracking-tight">{title}</h1>
      <p className="text-muted-foreground mb-5">{description}</p>

      <ol className="bg-card w-full space-y-3 rounded-xl border p-5 text-left">
        {steps.map((step) => (
          <li key={step.label} className="flex items-center gap-3">
            <StepIcon status={step.status} />
            <span
              className={
                step.status === "active"
                  ? "text-foreground text-sm font-medium"
                  : step.status === "done"
                    ? "text-muted-foreground text-sm"
                    : "text-muted-foreground/60 text-sm"
              }
            >
              {step.label}
            </span>
          </li>
        ))}
      </ol>
    </motion.div>
  );
}

function StepIcon({ status }: { status: AwaitingStep["status"] }) {
  if (status === "done") {
    return (
      <div className="flex size-6 shrink-0 items-center justify-center rounded-full bg-green-500/15">
        <Check className="size-3.5 text-green-500" />
      </div>
    );
  }
  if (status === "active") {
    return (
      <div className="bg-primary/15 flex size-6 shrink-0 items-center justify-center rounded-full">
        <Loader2 className="text-primary size-3.5 animate-spin" />
      </div>
    );
  }
  return (
    <div className="border-muted-foreground/30 size-6 shrink-0 rounded-full border-2" />
  );
}
