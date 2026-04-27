import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { NewCardSection } from "@/components/NewCardSection";

export function AddPaymentMethodModal({
  replace = false,
}: {
  replace?: boolean;
}) {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();

  function handleSuccess() {
    toast.success("Payment method saved");
    setOpen(false);
    queryClient.invalidateQueries({ queryKey: ["stripe", "payment-method"] });
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant={replace ? "outline" : "default"}>
          {replace ? "Replace" : "Add Payment Method"}
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Add Payment Method</DialogTitle>
        </DialogHeader>
        {open && <NewCardSection onConfirmed={handleSuccess} />}
      </DialogContent>
    </Dialog>
  );
}
