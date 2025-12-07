"use client";

import { useSearchParams } from "next/navigation";
import Link from "next/link";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/shared/ui/card";
import { Button } from "@/shared/ui/button";

const errorMessages: Record<string, string> = {
  Configuration: "There is a problem with the server configuration.",
  AccessDenied: "You do not have permission to sign in.",
  Verification: "The verification token has expired or has already been used.",
  Default: "An error occurred during authentication.",
};

export default function AuthErrorPage() {
  const searchParams = useSearchParams();
  const error = searchParams.get("error") ?? "Default";

  const errorMessage = errorMessages[error] ?? errorMessages.Default;

  return (
    <div className="flex justify-center">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>Authentication Error</CardTitle>
          <CardDescription>
            We encountered an issue while trying to authenticate you.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="rounded-lg bg-destructive/10 p-4 text-sm text-destructive">
            {errorMessage}
          </div>
        </CardContent>
        <CardFooter>
          <Button asChild className="w-full">
            <Link href="/login">Back to Login</Link>
          </Button>
        </CardFooter>
      </Card>
    </div>
  );
}
