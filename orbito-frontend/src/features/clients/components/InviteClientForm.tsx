"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/shared/ui/form";
import { Input } from "@/shared/ui/input";
import { Button } from "@/shared/ui/button";
import {
  InviteClientSchema,
  type InviteClientFormData,
} from "../schemas/invite-client.schema";
import { useInviteClient } from "../hooks/useInviteClient";

interface InviteClientFormProps {
  onSuccess?: () => void;
}

export function InviteClientForm({ onSuccess }: InviteClientFormProps) {
  const inviteMutation = useInviteClient();

  const form = useForm<InviteClientFormData>({
    resolver: zodResolver(InviteClientSchema),
    defaultValues: {
      email: "",
      firstName: "",
      lastName: "",
      companyName: "",
    },
  });

  const onSubmit = async (data: InviteClientFormData) => {
    await inviteMutation.mutateAsync({ data });
    form.reset();
    onSuccess?.();
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="email"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Email</FormLabel>
              <FormControl>
                <Input placeholder="klient@firma.pl" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="grid grid-cols-2 gap-4">
          <FormField
            control={form.control}
            name="firstName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Imię</FormLabel>
                <FormControl>
                  <Input placeholder="Jan" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="lastName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Nazwisko</FormLabel>
                <FormControl>
                  <Input placeholder="Kowalski" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="companyName"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Firma (opcjonalnie)</FormLabel>
              <FormControl>
                <Input placeholder="Nazwa firmy" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end pt-2">
          <Button type="submit" disabled={inviteMutation.isPending}>
            {inviteMutation.isPending ? "Wysyłanie..." : "Wyślij zaproszenie"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
