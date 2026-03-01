import { z } from "zod";

export const InviteClientSchema = z.object({
  email: z.string().email("Podaj prawidłowy email"),
  firstName: z.string().min(1, "Imię jest wymagane").max(100),
  lastName: z.string().min(1, "Nazwisko jest wymagane").max(100),
  companyName: z.string().max(200).optional(),
});

export type InviteClientFormData = z.infer<typeof InviteClientSchema>;
