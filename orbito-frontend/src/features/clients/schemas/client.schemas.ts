import { z } from "zod";

/**
 * Schema for creating a new client
 * Supports two scenarios:
 * A) Client with existing User account (userId)
 * B) Direct client without User account (directEmail + names)
 */

// Base schema with common optional fields
const baseClientSchema = z.object({
  companyName: z.string().optional(),
  phone: z.string().optional(),
});

// Scenario A: Client with existing User account
const userClientSchema = baseClientSchema.extend({
  clientType: z.literal("user"),
  userId: z.string().min(1, "User is required"),
  directEmail: z.undefined(),
  directFirstName: z.undefined(),
  directLastName: z.undefined(),
});

// Scenario B: Direct client (without User account)
const directClientSchema = baseClientSchema.extend({
  clientType: z.literal("direct"),
  userId: z.undefined(),
  directEmail: z.string().email("Invalid email address"),
  directFirstName: z.string().min(1, "First name is required"),
  directLastName: z.string().min(1, "Last name is required"),
});

// Discriminated union for create
export const CreateClientSchema = z.discriminatedUnion("clientType", [
  userClientSchema,
  directClientSchema,
]);

// Schema for updating a client
export const UpdateClientSchema = z.object({
  companyName: z.string().optional(),
  phone: z.string().optional(),
  directEmail: z.string().email("Invalid email address").optional().or(z.literal("")),
  directFirstName: z.string().optional(),
  directLastName: z.string().optional(),
});

export type CreateClientInput = z.infer<typeof CreateClientSchema>;
export type UpdateClientInput = z.infer<typeof UpdateClientSchema>;
