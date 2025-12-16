"use client";

import { useGetApiClientsId } from "@/core/api/generated/clients/clients";
import type { ClientDto, UpdateClientCommand } from "@/core/api/generated/models";
import { ClientForm } from "@/features/clients/components/ClientForm";
import { useClientMutations } from "@/features/clients/hooks/useClientMutations";
import type { UpdateClientInput } from "@/features/clients/schemas/client.schemas";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { Button } from "@/shared/ui/button";
import { Skeleton } from "@/shared/ui/skeleton";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { use } from "react";

interface EditClientPageProps {
  params: Promise<{ id: string }>;
}

export default function EditClientPage({ params }: EditClientPageProps) {
  const { id } = use(params);

  const { data, isLoading, error } = useGetApiClientsId(id) as {
    data: ClientDto | undefined;
    isLoading: boolean;
    error: Error | null;
  };

  const { updateClient, isUpdating } = useClientMutations();

  const handleSubmit = (formData: UpdateClientInput) => {
    // Transform form data to UpdateClientCommand
    const command: UpdateClientCommand = {
      id,
      companyName: formData.companyName || null,
      phone: formData.phone || null,
      directEmail: formData.directEmail || null,
      directFirstName: formData.directFirstName || null,
      directLastName: formData.directLastName || null,
    };

    updateClient(id, command);
  };

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-10 w-64" />
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-32" />
          </CardHeader>
          <CardContent className="space-y-4">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </CardContent>
        </Card>
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-4">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href="/dashboard/clients">
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back to Clients
            </Link>
          </Button>
        </div>
        <Card>
          <CardContent className="pt-6">
            <p className="text-destructive">
              Failed to load client: {error.message}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="space-y-4">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href="/dashboard/clients">
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back to Clients
            </Link>
          </Button>
        </div>
        <Card>
          <CardContent className="pt-6">
            <p className="text-muted-foreground">Client not found</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" asChild>
          <Link href={`/dashboard/clients/${id}`}>
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Client
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Edit Client</h1>
          <p className="text-muted-foreground">
            Update client information for{" "}
            {data.fullName ||
              `${data.directFirstName} ${data.directLastName}` ||
              "client"}
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Client Information</CardTitle>
        </CardHeader>
        <CardContent>
          <ClientForm
            mode="edit"
            defaultValues={data}
            onSubmit={handleSubmit}
            isSubmitting={isUpdating}
          />
        </CardContent>
      </Card>
    </div>
  );
}
