"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/ui/dialog";
import { Button } from "@/shared/ui/button";
import { useResumeSubscription } from "../../hooks/useSubscriptionMutations";
import {
  ResumeSubscriptionSchema,
  type ResumeSubscriptionInput,
} from "../../schemas/subscription.schemas";

interface ResumeSubscriptionDialogProps {
  subscriptionId: string;
  clientId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function ResumeSubscriptionDialog({
  subscriptionId,
  clientId,
  open,
  onOpenChange,
}: ResumeSubscriptionDialogProps) {
  const resumeMutation = useResumeSubscription(subscriptionId);

  const { handleSubmit, reset } = useForm<ResumeSubscriptionInput>({
    resolver: zodResolver(ResumeSubscriptionSchema),
    defaultValues: {
      clientId,
    },
  });

  const onSubmit = async (data: ResumeSubscriptionInput) => {
    await resumeMutation.mutateAsync({
      id: subscriptionId,
      data: {
        clientId: data.clientId,
      },
    });
    reset();
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Resume Subscription</DialogTitle>
          <DialogDescription>
            Resuming this subscription will restart billing immediately.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)}>
          <DialogFooter className="mt-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Close
            </Button>
            <Button type="submit" disabled={resumeMutation.isPending}>
              {resumeMutation.isPending ? "Resuming..." : "Resume Subscription"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
