"use client";

import { ClientForm } from "@/features/clients/components/ClientForm";
import { useClientMutations } from "@/features/clients/hooks/useClientMutations";
import type { CreateClientInput } from "@/features/clients/schemas/client.schemas";
import type { CreateClientCommand } from "@/core/api/generated/models";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { Button } from "@/shared/ui/button";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";

export default function NewClientPage() {
  const { createClient, isCreating } = useClientMutations();

  const handleSubmit = (data: CreateClientInput) => {
    // Transform form data to CreateClientCommand
    const command: CreateClientCommand =
      data.clientType === "user"
        ? {
            userId: data.userId,
            companyName: data.companyName || null,
            phone: data.phone || null,
            directEmail: null,
            directFirstName: null,
            directLastName: null,
          }
        : {
            userId: null,
            directEmail: data.directEmail,
            directFirstName: data.directFirstName,
            directLastName: data.directLastName,
            companyName: data.companyName || null,
            phone: data.phone || null,
          };

    createClient(command);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" asChild>
          <Link href="/dashboard/clients">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Clients
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">New Client</h1>
          <p className="text-muted-foreground">
            Create a new client for your organization
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Client Information</CardTitle>
        </CardHeader>
        <CardContent>
          <ClientForm
            mode="create"
            onSubmit={handleSubmit}
            isSubmitting={isCreating}
          />
        </CardContent>
      </Card>
    </div>
  );
}
