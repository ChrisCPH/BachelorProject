import { z } from "zod";

export const RegisterSchema = z.object({
    userName: z.string().min(1, "Username is required"),
    email: z.string().email("Invalid email"),
    password: z.string().min(6, "Password must be at least 6 characters"),
    confirmPassword: z.string(),
    preferredDistance: z.enum(["km", "mi"], {
        errorMap: () => ({ message: "Preferred distance unit is required" }),
    }),
    preferredWeight: z.enum(["kg", "lbs"], {
        errorMap: () => ({ message: "Preferred weight unit is required" }),
    }),
}).refine((data) => data.password === data.confirmPassword, {
    message: "Passwords must match",
    path: ["confirmPassword"],
});

export type RegisterFormData = z.infer<typeof RegisterSchema>;
