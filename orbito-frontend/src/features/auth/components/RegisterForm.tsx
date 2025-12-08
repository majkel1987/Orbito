"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { toast } from "sonner";
import { AxiosError } from "axios";
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

export function RegisterForm() {
  const router = useRouter();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<RegisterInput>({
    resolver: zodResolver(RegisterSchema),
  });

  const { mutate: registerProvider } = usePostApiAccountRegisterProvider({
    mutation: {
      onSuccess: () => {
        toast.success("Registration successful! Please log in.");
        router.push("/login");
      },
      onError: (error: AxiosError) => {
        console.error("Registration error:", error);
        console.error("Error response:", error.response?.data);

        const errorData = error.response?.data as
          | { message?: string; errors?: string[] }
          | undefined;

        const errorMessage =
          errorData?.message ||
          error.message ||
          "An error occurred during registration";

        toast.error(errorMessage);

        if (errorData?.errors && errorData.errors.length > 0) {
          errorData.errors.forEach((err) => {
            toast.error(err);
          });
        }
      },
    },
  });

  const onSubmit = async (data: RegisterInput) => {
    registerProvider({
      data: {
        email: data.email,
        password: data.password,
        firstName: data.firstName,
        lastName: data.lastName,
        businessName: data.businessName,
        subdomainSlug: data.subdomainSlug,
      },
    });
  };

  return (
    <Card className="w-full max-w-md">
      <CardHeader>
        <CardTitle>Register</CardTitle>
        <CardDescription>
          Create a new account to get started
        </CardDescription>
      </CardHeader>
      <form onSubmit={handleSubmit(onSubmit)}>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="firstName">First Name</Label>
              <Input
                id="firstName"
                type="text"
                placeholder="John"
                {...register("firstName")}
                disabled={isSubmitting}
                autoComplete="given-name"
              />
              {errors.firstName && (
                <p className="text-sm text-destructive">
                  {errors.firstName.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="lastName">Last Name</Label>
              <Input
                id="lastName"
                type="text"
                placeholder="Doe"
                {...register("lastName")}
                disabled={isSubmitting}
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
            <Label htmlFor="businessName">Business Name</Label>
            <Input
              id="businessName"
              type="text"
              placeholder="Acme Corporation"
              {...register("businessName")}
              disabled={isSubmitting}
              autoComplete="organization"
            />
            {errors.businessName && (
              <p className="text-sm text-destructive">
                {errors.businessName.message}
              </p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="subdomainSlug">Subdomain</Label>
            <div className="flex items-center gap-2">
              <Input
                id="subdomainSlug"
                type="text"
                placeholder="acme"
                {...register("subdomainSlug")}
                disabled={isSubmitting}
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
              Only lowercase letters, numbers, and hyphens. Cannot start or end
              with a hyphen.
            </p>
          </div>

          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              placeholder="you@example.com"
              {...register("email")}
              disabled={isSubmitting}
              autoComplete="email"
            />
            {errors.email && (
              <p className="text-sm text-destructive">{errors.email.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="password">Password</Label>
            <Input
              id="password"
              type="password"
              placeholder="••••••••"
              {...register("password")}
              disabled={isSubmitting}
              autoComplete="new-password"
            />
            {errors.password && (
              <p className="text-sm text-destructive">
                {errors.password.message}
              </p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="confirmPassword">Confirm Password</Label>
            <Input
              id="confirmPassword"
              type="password"
              placeholder="••••••••"
              {...register("confirmPassword")}
              disabled={isSubmitting}
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
          <Button type="submit" className="w-full" disabled={isSubmitting}>
            {isSubmitting ? "Registering..." : "Register"}
          </Button>
          <p className="text-sm text-muted-foreground">
            Already have an account?{" "}
            <Link href="/login" className="text-primary hover:underline">
              Login
            </Link>
          </p>
        </CardFooter>
      </form>
    </Card>
  );
}
