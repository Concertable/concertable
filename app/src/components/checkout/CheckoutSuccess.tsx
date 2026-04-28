import type { ReactNode } from "react";
import { motion } from "framer-motion";
import { CheckCircle } from "lucide-react";

interface Props {
  title: string;
  description: ReactNode;
  details?: ReactNode;
  footer?: ReactNode;
}

export function CheckoutSuccess({
  title,
  description,
  details,
  footer,
}: Props) {
  return (
    <motion.div
      initial={{ opacity: 0, y: 8 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className="mx-auto flex min-h-[60vh] max-w-md flex-col items-center justify-center px-4 py-12 text-center"
    >
      <motion.div
        initial={{ scale: 0 }}
        animate={{ scale: 1 }}
        transition={{ type: "spring", stiffness: 260, damping: 18, delay: 0.1 }}
        className="mb-6"
      >
        <CheckCircle className="size-16 text-green-500" />
      </motion.div>
      <h1 className="mb-2 text-2xl font-bold tracking-tight">{title}</h1>
      <p className="text-muted-foreground mb-6">{description}</p>
      {details && <div className="w-full">{details}</div>}
      {footer && (
        <div className="text-muted-foreground mt-6 text-sm">{footer}</div>
      )}
    </motion.div>
  );
}
