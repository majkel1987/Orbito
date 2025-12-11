"use client";

import { use, useState } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/shared/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/ui/card";
import { usePostApiTeamMembersAccept } from "@/core/api/generated/team-members/team-members";
import { toast } from "sonner";

interface Props {
  params: Promise<{ token: string }>;
}

export default function AcceptInvitationPage({ params }: Props) {
  const { token } = use(params);
  const router = useRouter();
  const [isAccepting, setIsAccepting] = useState(false);

  const acceptMutation = usePostApiTeamMembersAccept({
    mutation: {
      onSuccess: () => {
        toast.success("Invitation accepted! Please login to continue.");
        router.push("/login");
      },
      onError: (error: Error) => {
        toast.error(
          error.message || "Failed to accept invitation. It may have expired."
        );
        setIsAccepting(false);
      },
    },
  });

  async function handleAccept() {
    setIsAccepting(true);
    acceptMutation.mutate({
      data: {
        token,
      },
    });
  }

  return (
    <div className="flex min-h-screen items-center justify-center p-4">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>Team Invitation</CardTitle>
          <CardDescription>
            You have been invited to join a team. Click the button below to
            accept the invitation.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Button
            onClick={handleAccept}
            disabled={isAccepting}
            className="w-full"
          >
            {isAccepting ? "Accepting..." : "Accept Invitation"}
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
