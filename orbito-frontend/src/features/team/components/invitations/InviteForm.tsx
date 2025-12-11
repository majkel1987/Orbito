"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { InviteSchema, InviteInput } from "../../schemas/invitation.schemas";
import { useInvitations } from "../../hooks/useInvitations";
import { Button } from "@/shared/ui/button";
import { Input } from "@/shared/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui/select";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/shared/ui/form";
import { TeamMemberRole } from "@/core/api/generated/models";
import { getTeamMemberRoleName } from "../../types/team.types";

export function InviteForm() {
  const { sendInvite, isSending } = useInvitations();

  const form = useForm<InviteInput>({
    resolver: zodResolver(InviteSchema),
    defaultValues: {
      email: "",
      role: TeamMemberRole.NUMBER_3, // Default to Member
    },
  });

  function onSubmit(data: InviteInput) {
    sendInvite({
      data: {
        email: data.email,
        role: data.role,
        firstName: null,
        lastName: null,
        message: null,
      },
    });
    form.reset();
  }

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
                <Input placeholder="colleague@example.com" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="role"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Role</FormLabel>
              <Select
                onValueChange={(value) => field.onChange(parseInt(value))}
                defaultValue={field.value?.toString()}
              >
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder="Select a role" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  <SelectItem value={TeamMemberRole.NUMBER_1.toString()}>
                    {getTeamMemberRoleName(TeamMemberRole.NUMBER_1)}
                  </SelectItem>
                  <SelectItem value={TeamMemberRole.NUMBER_3.toString()}>
                    {getTeamMemberRoleName(TeamMemberRole.NUMBER_3)}
                  </SelectItem>
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button type="submit" disabled={isSending}>
          {isSending ? "Sending..." : "Send Invitation"}
        </Button>
      </form>
    </Form>
  );
}
