import { z } from "zod";

export const RegisterSchema = z.object({
    userName: z.string(),
    email: z.string().email(),
    password: z.string(),
    confirmPassword: z.string(),
});

export type RegisterFormData = z.infer<typeof RegisterSchema>;