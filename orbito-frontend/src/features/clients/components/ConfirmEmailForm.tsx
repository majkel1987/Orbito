"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import Link from "next/link";
import { usePostApiClientsConfirmEmail } from "@/core/api/generated/clients/clients";
import {
  ConfirmEmailSchema,
  type ConfirmEmailFormData,
} from "@/features/clients/schemas/confirm-email.schema";
import { Button } from "@/shared/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/shared/ui/form";
import { Input } from "@/shared/ui/input";
import { CheckCircle, Loader2 } from "lucide-react";

interface ConfirmEmailFormProps {
  token: string;
}

export function ConfirmEmailForm({ token }: ConfirmEmailFormProps) {
  const [isSuccess, setIsSuccess] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const form = useForm<ConfirmEmailFormData>({
    resolver: zodResolver(ConfirmEmailSchema),
    defaultValues: {
      token,
      password: "",
      confirmPassword: "",
    },
  });

  const mutation = usePostApiClientsConfirmEmail({
    mutation: {
      onSuccess: () => {
        setIsSuccess(true);
        setErrorMessage(null);
      },
      onError: (error) => {
        if (error instanceof Error) {
          setErrorMessage(error.message);
        } else {
          const problemDetails = error as { title?: string; detail?: string };
          setErrorMessage(
            problemDetails.detail ??
              problemDetails.title ??
              "Wystąpił błąd podczas aktywacji konta."
          );
        }
      },
    },
  });

  const onSubmit = async (data: ConfirmEmailFormData) => {
    setErrorMessage(null);
    await mutation.mutateAsync({
      data: {
        token: data.token,
        password: data.password,
      },
    });
  };

  if (isSuccess) {
    return (
      <div className="space-y-6 text-center">
        <div className="flex justify-center">
          <CheckCircle className="h-16 w-16 text-green-500" />
        </div>
        <div className="space-y-2">
          <h2 className="text-xl font-semibold">Konto aktywowane!</h2>
          <p className="text-muted-foreground">
            Twoje konto zostało pomyślnie aktywowane. Możesz się teraz zalogować
            do portalu klienta.
          </p>
        </div>
        <Button asChild className="w-full">
          <Link href="/login">Zaloguj się</Link>
        </Button>
      </div>
    );
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        {errorMessage && (
          <div className="rounded-md bg-destructive/10 px-4 py-3 text-sm text-destructive">
            {errorMessage}
          </div>
        )}

        <FormField
          control={form.control}
          name="password"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Hasło</FormLabel>
              <FormControl>
                <Input
                  type="password"
                  placeholder="Minimum 8 znaków"
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="confirmPassword"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Powtórz hasło</FormLabel>
              <FormControl>
                <Input type="password" placeholder="Powtórz hasło" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button type="submit" className="w-full" disabled={mutation.isPending}>
          {mutation.isPending && (
            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
          )}
          Aktywuj konto
        </Button>
      </form>
    </Form>
  );
}
