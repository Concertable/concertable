import type { ReactNode } from "react";
import { CheckCircle } from "lucide-react";

interface Props {
  title: string;
  description: ReactNode;
  details?: ReactNode;
  footer?: ReactNode;
}

export function CheckoutSuccessView({
  title,
  description,
  details,
  footer,
}: Props) {
  return (
    <div className="mx-auto flex min-h-[60vh] max-w-md flex-col items-center justify-center px-4 py-12 text-center">
      <CheckCircle className="mb-6 size-16 text-green-500" />
      <h1 className="mb-2 text-2xl font-bold tracking-tight">{title}</h1>
      <p className="text-muted-foreground mb-6">{description}</p>
      {details && <div className="w-full">{details}</div>}
      {footer && (
        <div className="text-muted-foreground mt-6 text-sm">{footer}</div>
      )}
    </div>
  );
}
