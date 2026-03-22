"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { toast } from "sonner";
import { AxiosError } from "axios";
import { ArrowLeft } from "lucide-react";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/shared/ui/card";
import { Input } from "@/shared/ui/input";
import { Button } from "@/shared/ui/button";
import { Label } from "@/shared/ui/label";
import { RegisterSchema, type RegisterInput } from "../schemas/auth.schemas";
import { usePostApiAccountRegisterProvider } from "@/core/api/generated/account/account";
import { PlanSelectionStep } from "./PlanSelectionStep";

type Step = "plan" | "details";

export function RegisterForm() {
  const router = useRouter();
  const [step, setStep] = useState<Step>("plan");
  const [selectedPlanId, setSelectedPlanId] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
  } = useForm<RegisterInput>({
    resolver: zodResolver(RegisterSchema),
  });

  const { mutate: registerProvider, isPending } = usePostApiAccountRegisterProvider({
    mutation: {
      onSuccess: () => {
        toast.success("Rejestracja zakończona pomyślnie! Możesz się teraz zalogować.");
        router.push("/login");
      },
      onError: (error: AxiosError) => {
        console.error("Registration error:", error);
        console.error("Error response:", error.response?.data);

        const errorData = error.response?.data as
          | { message?: string; errors?: string[]; validationErrors?: Record<string, string[]> }
          | undefined;

        const errorMessage =
          errorData?.message ||
          error.message ||
          "Wystąpił błąd podczas rejestracji";

        toast.error(errorMessage);

        if (errorData?.errors && errorData.errors.length > 0) {
          errorData.errors.slice(0, 3).forEach((err) => {
            toast.error(err);
          });
        }
      },
    },
  });

  const handlePlanSelect = (planId: string) => {
    setSelectedPlanId(planId);
    setValue("selectedPlatformPlanId", planId);
  };

  const handleContinueToDetails = () => {
    if (selectedPlanId) {
      setStep("details");
    }
  };

  const handleBackToPlan = () => {
    setStep("plan");
  };

  const onSubmit = async (data: RegisterInput) => {
    registerProvider({
      data: {
        email: data.email,
        password: data.password,
        firstName: data.firstName,
        lastName: data.lastName,
        businessName: data.businessName,
        subdomainSlug: data.subdomainSlug,
        selectedPlatformPlanId: selectedPlanId,
      },
    });
  };

  if (step === "plan") {
    return (
      <div className="w-full max-w-4xl mx-auto">
        <PlanSelectionStep
          selectedPlanId={selectedPlanId}
          onSelectPlan={handlePlanSelect}
          onContinue={handleContinueToDetails}
        />
        <p className="text-center text-sm text-muted-foreground mt-4">
          Masz już konto?{" "}
          <Link href="/login" className="text-primary hover:underline">
            Zaloguj się
          </Link>
        </p>
      </div>
    );
  }

  return (
    <Card className="w-full max-w-md">
      <CardHeader>
        <div className="flex items-center gap-2">
          <Button
            type="button"
            variant="ghost"
            size="icon"
            onClick={handleBackToPlan}
            className="h-8 w-8"
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <CardTitle>Uzupełnij dane</CardTitle>
            <CardDescription>
              Stwórz konto, aby rozpocząć okres próbny
            </CardDescription>
          </div>
        </div>
      </CardHeader>
      <form onSubmit={handleSubmit(onSubmit)}>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="firstName">Imię</Label>
              <Input
                id="firstName"
                type="text"
                placeholder="Jan"
                {...register("firstName")}
                disabled={isPending}
                autoComplete="given-name"
              />
              {errors.firstName && (
                <p className="text-sm text-destructive">
                  {errors.firstName.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="lastName">Nazwisko</Label>
              <Input
                id="lastName"
                type="text"
                placeholder="Kowalski"
                {...register("lastName")}
                disabled={isPending}
                autoComplete="family-name"
              />
              {errors.lastName && (
                <p className="text-sm text-destructive">
                  {errors.lastName.message}
                </p>
              )}
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="businessName">Nazwa firmy</Label>
            <Input
              id="businessName"
              type="text"
              placeholder="Moja Firma Sp. z o.o."
              {...register("businessName")}
              disabled={isPending}
              autoComplete="organization"
            />
            {errors.businessName && (
              <p className="text-sm text-destructive">
                {errors.businessName.message}
              </p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="subdomainSlug">Subdomena</Label>
            <div className="flex items-center gap-2">
              <Input
                id="subdomainSlug"
                type="text"
                placeholder="mojafirma"
                {...register("subdomainSlug")}
                disabled={isPending}
                className="flex-1"
              />
              <span className="text-sm text-muted-foreground whitespace-nowrap">
                .orbito.com
              </span>
            </div>
            {errors.subdomainSlug && (
              <p className="text-sm text-destructive">
                {errors.subdomainSlug.message}
              </p>
            )}
            <p className="text-xs text-muted-foreground">
              Tylko małe litery, cyfry i myślniki. Nie może zaczynać ani kończyć
              się myślnikiem.
            </p>
          </div>

          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              placeholder="jan@mojafirma.pl"
              {...register("email")}
              disabled={isPending}
              autoComplete="email"
            />
            {errors.email && (
              <p className="text-sm text-destructive">{errors.email.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="password">Hasło</Label>
            <Input
              id="password"
              type="password"
              placeholder="••••••••"
              {...register("password")}
              disabled={isPending}
              autoComplete="new-password"
            />
            {errors.password && (
              <p className="text-sm text-destructive">
                {errors.password.message}
              </p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="confirmPassword">Potwierdź hasło</Label>
            <Input
              id="confirmPassword"
              type="password"
              placeholder="••••••••"
              {...register("confirmPassword")}
              disabled={isPending}
              autoComplete="new-password"
            />
            {errors.confirmPassword && (
              <p className="text-sm text-destructive">
                {errors.confirmPassword.message}
              </p>
            )}
          </div>
        </CardContent>

        <CardFooter className="flex flex-col gap-4">
          <Button type="submit" className="w-full" disabled={isPending}>
            {isPending ? "Rejestracja..." : "Zarejestruj się"}
          </Button>
          <p className="text-sm text-muted-foreground">
            Masz już konto?{" "}
            <Link href="/login" className="text-primary hover:underline">
              Zaloguj się
            </Link>
          </p>
        </CardFooter>
      </form>
    </Card>
  );
}
