"use client";

import { useGetApiClientsId } from "@/core/api/generated/clients/clients";
import type { ClientDto } from "@/core/api/generated/models";
import { useClientMutations } from "@/features/clients/hooks/useClientMutations";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { Button } from "@/shared/ui/button";
import { Skeleton } from "@/shared/ui/skeleton";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/shared/ui/alert-dialog";
import Link from "next/link";
import { ArrowLeft, Trash2 } from "lucide-react";
import { use, useState } from "react";
import { useRouter } from "next/navigation";

interface DeleteClientPageProps {
  params: Promise<{ id: string }>;
}

export default function DeleteClientPage({ params }: DeleteClientPageProps) {
  const { id } = use(params);
  const router = useRouter();
  const [showDialog, setShowDialog] = useState(true);

  const { data, isLoading, error } = useGetApiClientsId(id) as {
    data: ClientDto | undefined;
    isLoading: boolean;
    error: Error | null;
  };

  const { deleteClient, isDeleting } = useClientMutations();

  const handleDelete = () => {
    deleteClient(id);
  };

  const handleCancel = () => {
    setShowDialog(false);
    router.back();
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
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-full" />
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
          <h1 className="text-3xl font-bold tracking-tight">Delete Client</h1>
          <p className="text-muted-foreground">
            Permanently delete this client and all associated data
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-destructive">
            <Trash2 className="inline h-5 w-5 mr-2" />
            Delete Confirmation
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-muted-foreground mb-4">
            You are about to delete:
          </p>
          <div className="rounded-lg border p-4 space-y-2">
            <p className="font-medium">
              {data.fullName ||
                `${data.directFirstName} ${data.directLastName}` ||
                "Unnamed Client"}
            </p>
            {data.companyName && (
              <p className="text-sm text-muted-foreground">
                {data.companyName}
              </p>
            )}
            <p className="text-sm text-muted-foreground">
              {data.directEmail || data.email || data.userEmail || "No email"}
            </p>
          </div>
          <p className="text-sm text-destructive mt-4">
            This action cannot be undone. All client data, subscriptions, and
            payments will be permanently removed.
          </p>
        </CardContent>
      </Card>

      <AlertDialog open={showDialog} onOpenChange={setShowDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This will permanently delete the client{" "}
              <span className="font-semibold">
                {data.fullName ||
                  `${data.directFirstName} ${data.directLastName}` ||
                  "this client"}
              </span>{" "}
              and all associated data. This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={handleCancel} disabled={isDeleting}>
              Cancel
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              disabled={isDeleting}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {isDeleting ? "Deleting..." : "Delete Client"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
