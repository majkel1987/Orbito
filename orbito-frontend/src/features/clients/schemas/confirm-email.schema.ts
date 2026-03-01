import { z } from "zod";

export const ConfirmEmailSchema = z
  .object({
    token: z.string().min(1),
    password: z
      .string()
      .min(8, "Hasło musi mieć minimum 8 znaków")
      .regex(/[A-Z]/, "Hasło musi zawierać wielką literę")
      .regex(/[0-9]/, "Hasło musi zawierać cyfrę")
      .regex(/[^a-zA-Z0-9]/, "Hasło musi zawierać znak specjalny"),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Hasła muszą być identyczne",
    path: ["confirmPassword"],
  });

export type ConfirmEmailFormData = z.infer<typeof ConfirmEmailSchema>;
