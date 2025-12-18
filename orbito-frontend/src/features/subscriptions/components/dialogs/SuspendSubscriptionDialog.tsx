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
import { useSuspendSubscription } from "../../hooks/useSubscriptionMutations";
import {
  SuspendSubscriptionSchema,
  type SuspendSubscriptionInput,
} from "../../schemas/subscription.schemas";

interface SuspendSubscriptionDialogProps {
  subscriptionId: string;
  clientId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function SuspendSubscriptionDialog({
  subscriptionId,
  clientId,
  open,
  onOpenChange,
}: SuspendSubscriptionDialogProps) {
  const suspendMutation = useSuspendSubscription(subscriptionId);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<SuspendSubscriptionInput>({
    resolver: zodResolver(SuspendSubscriptionSchema),
    defaultValues: {
      clientId,
      reason: "",
    },
  });

  const onSubmit = async (data: SuspendSubscriptionInput) => {
    await suspendMutation.mutateAsync({
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
          <DialogTitle>Suspend Subscription</DialogTitle>
          <DialogDescription>
            Suspending this subscription will pause billing until it is resumed.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="space-y-4 py-4">
            <div>
              <Label htmlFor="reason">Reason (optional)</Label>
              <Textarea
                id="reason"
                placeholder="Why is this subscription being suspended?"
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
            <Button type="submit" disabled={suspendMutation.isPending}>
              {suspendMutation.isPending ? "Suspending..." : "Suspend Subscription"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
