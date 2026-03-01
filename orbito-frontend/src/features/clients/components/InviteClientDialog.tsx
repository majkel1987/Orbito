"use client";

import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/shared/ui/dialog";
import { Button } from "@/shared/ui/button";
import { UserPlus } from "lucide-react";
import { InviteClientForm } from "./InviteClientForm";

export function InviteClientDialog() {
  const [open, setOpen] = useState(false);

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button>
          <UserPlus className="mr-2 h-4 w-4" />
          Zaproś klienta
        </Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Zaproś klienta</DialogTitle>
          <DialogDescription>
            Klient otrzyma email z linkiem do aktywacji konta.
          </DialogDescription>
        </DialogHeader>
        <InviteClientForm onSuccess={() => setOpen(false)} />
      </DialogContent>
    </Dialog>
  );
}
