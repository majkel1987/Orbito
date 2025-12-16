"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/shared/ui/button";
import { Input } from "@/shared/ui/input";
import { Label } from "@/shared/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui/select";
import {
  CreateClientSchema,
  UpdateClientSchema,
  type CreateClientInput,
  type UpdateClientInput,
} from "../schemas/client.schemas";
import type { ClientDto } from "@/core/api/generated/models";
import { useState } from "react";

type ClientFormProps =
  | {
      mode: "create";
      defaultValues?: never;
      onSubmit: (data: CreateClientInput) => void;
      isSubmitting?: boolean;
    }
  | {
      mode: "edit";
      defaultValues: ClientDto;
      onSubmit: (data: UpdateClientInput) => void;
      isSubmitting?: boolean;
    };

export function ClientForm({
  mode,
  defaultValues,
  onSubmit,
  isSubmitting,
}: ClientFormProps) {
  const [clientType, setClientType] = useState<"user" | "direct">(
    defaultValues?.userId ? "user" : "direct"
  );

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
  } = useForm<CreateClientInput | UpdateClientInput>({
    resolver: zodResolver(
      mode === "create" ? CreateClientSchema : UpdateClientSchema
    ),
    defaultValues:
      mode === "edit" && defaultValues
        ? {
            companyName: defaultValues.companyName || "",
            phone: defaultValues.phone || "",
            directEmail: defaultValues.directEmail || "",
            directFirstName: defaultValues.directFirstName || "",
            directLastName: defaultValues.directLastName || "",
          }
        : mode === "create"
          ? {
              clientType: "direct",
              companyName: "",
              phone: "",
              directEmail: "",
              directFirstName: "",
              directLastName: "",
            }
          : undefined,
  });

  // For create mode, set clientType in form
  const handleClientTypeChange = (value: "user" | "direct") => {
    setClientType(value);
    if (mode === "create") {
      setValue("clientType" as keyof CreateClientInput, value);
    }
  };

  const handleFormSubmit = (data: CreateClientInput | UpdateClientInput) => {
    if (mode === "create") {
      (onSubmit as (data: CreateClientInput) => void)(data as CreateClientInput);
    } else {
      (onSubmit as (data: UpdateClientInput) => void)(data as UpdateClientInput);
    }
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-6">
      {/* Client Type Selection - only for create mode */}
      {mode === "create" && (
        <div className="space-y-2">
          <Label htmlFor="clientType">Client Type</Label>
          <Select
            value={clientType}
            onValueChange={handleClientTypeChange}
            disabled={isSubmitting}
          >
            <SelectTrigger>
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="direct">
                Direct Client (No User Account)
              </SelectItem>
              <SelectItem value="user">
                Client with User Account
              </SelectItem>
            </SelectContent>
          </Select>
          <p className="text-sm text-muted-foreground">
            {clientType === "direct"
              ? "Create a client without linking to an existing user account"
              : "Link this client to an existing user account"}
          </p>
        </div>
      )}

      {/* User ID - only for "user" type in create mode */}
      {mode === "create" && clientType === "user" && (
        <div className="space-y-2">
          <Label htmlFor="userId">User ID</Label>
          <Input
            id="userId"
            {...register("userId" as keyof CreateClientInput)}
            placeholder="Enter user ID"
            disabled={isSubmitting}
          />
          {"userId" in errors && errors.userId && (
            <p className="text-sm text-destructive">{errors.userId.message}</p>
          )}
        </div>
      )}

      {/* Direct Client Fields - for "direct" type in create mode, or always in edit mode */}
      {((mode === "create" && clientType === "direct") || mode === "edit") && (
        <>
          <div className="space-y-2">
            <Label htmlFor="directEmail">Email</Label>
            <Input
              id="directEmail"
              type="email"
              {...register("directEmail")}
              placeholder="client@example.com"
              disabled={isSubmitting}
            />
            {errors.directEmail && (
              <p className="text-sm text-destructive">
                {errors.directEmail.message}
              </p>
            )}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="directFirstName">First Name</Label>
              <Input
                id="directFirstName"
                {...register("directFirstName")}
                placeholder="John"
                disabled={isSubmitting}
              />
              {errors.directFirstName && (
                <p className="text-sm text-destructive">
                  {errors.directFirstName.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="directLastName">Last Name</Label>
              <Input
                id="directLastName"
                {...register("directLastName")}
                placeholder="Doe"
                disabled={isSubmitting}
              />
              {errors.directLastName && (
                <p className="text-sm text-destructive">
                  {errors.directLastName.message}
                </p>
              )}
            </div>
          </div>
        </>
      )}

      {/* Common Optional Fields */}
      <div className="space-y-2">
        <Label htmlFor="companyName">Company Name (Optional)</Label>
        <Input
          id="companyName"
          {...register("companyName")}
          placeholder="Acme Corporation"
          disabled={isSubmitting}
        />
        {errors.companyName && (
          <p className="text-sm text-destructive">
            {errors.companyName.message}
          </p>
        )}
      </div>

      <div className="space-y-2">
        <Label htmlFor="phone">Phone (Optional)</Label>
        <Input
          id="phone"
          {...register("phone")}
          placeholder="+48 123 456 789"
          disabled={isSubmitting}
        />
        {errors.phone && (
          <p className="text-sm text-destructive">{errors.phone.message}</p>
        )}
      </div>

      {/* Form Actions */}
      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting
            ? mode === "create"
              ? "Creating..."
              : "Updating..."
            : mode === "create"
              ? "Create Client"
              : "Update Client"}
        </Button>
      </div>
    </form>
  );
}
