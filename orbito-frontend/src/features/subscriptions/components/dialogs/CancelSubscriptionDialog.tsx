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
import { Label } from "@/shared/ui/label";
import { Textarea } from "@/shared/ui/textarea";
import { useCancelSubscription } from "../../hooks/useSubscriptionMutations";
import {
  CancelSubscriptionSchema,
  type CancelSubscriptionInput,
} from "../../schemas/subscription.schemas";

interface CancelSubscriptionDialogProps {
  subscriptionId: string;
  clientId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function CancelSubscriptionDialog({
  subscriptionId,
  clientId,
  open,
  onOpenChange,
}: CancelSubscriptionDialogProps) {
  const cancelMutation = useCancelSubscription(subscriptionId);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CancelSubscriptionInput>({
    resolver: zodResolver(CancelSubscriptionSchema),
    defaultValues: {
      clientId,
      reason: "",
    },
  });

  const onSubmit = async (data: CancelSubscriptionInput) => {
    await cancelMutation.mutateAsync({
      id: subscriptionId,
      data: {
        clientId: data.clientId,
        reason: data.reason || null,
      },
    });
    reset();
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Cancel Subscription</DialogTitle>
          <DialogDescription>
            Are you sure you want to cancel this subscription? This action cannot be
            undone.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="space-y-4 py-4">
            <div>
              <Label htmlFor="reason">Reason (optional)</Label>
              <Textarea
                id="reason"
                placeholder="Why is this subscription being cancelled?"
                {...register("reason")}
                className="mt-2"
              />
              {errors.reason && (
                <p className="text-sm text-red-500 mt-1">{errors.reason.message}</p>
              )}
            </div>
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Close
            </Button>
            <Button
              type="submit"
              variant="destructive"
              disabled={cancelMutation.isPending}
            >
              {cancelMutation.isPending ? "Cancelling..." : "Cancel Subscription"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
