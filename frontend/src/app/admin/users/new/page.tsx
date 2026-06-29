"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import { useCreateUser } from "@/api/hooks/useCreateUser"
import type { CreateUserCommand } from "@/api/generated"

const userSchema = z.object({
  username: z.string().min(1, "Username is required"),
  email: z.string().email("Invalid email address"),
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  role: z.string().min(1, "Role is required"),
  isActive: z.string().min(1, "Status is required"),
})

type UserForm = z.infer<typeof userSchema>

const roleOptions = [
  { label: "Admin", value: "ADMIN" },
  { label: "Manager", value: "MANAGER" },
  { label: "Agent", value: "AGENT" },
  { label: "Viewer", value: "VIEWER" },
]

const statusOptions = [
  { label: "Active", value: "true" },
  { label: "Inactive", value: "false" },
]

export default function NewUserPage() {
  const router = useRouter()
  const createUser = useCreateUser()

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<UserForm>({
    resolver: zodResolver(userSchema),
  })

  const onSubmit = (data: UserForm) => {
    createUser.mutate(
      {
        username: data.username,
        email: data.email,
        firstName: data.firstName,
        lastName: data.lastName,
        phoneNumber: null,
        countryCode: null,
        externalId: null,
      } as CreateUserCommand,
      {
        onSuccess: () => {
          if (data.role) {
            console.warn("Role assignment requires a separate API call (POST /users/{id}/roles) after creation — not implemented yet")
          }
          toast({ title: "User created", description: "User has been created successfully." })
          router.push("/admin")
        },
        onError: () => {
          toast({ title: "Error", description: "Failed to create user.", variant: "destructive" })
        },
      }
    )
  }

  return (
    <FormPageLayout title="New User" backHref="/admin" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="User Information">
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Username"
            required
            error={errors.username}
            registration={register("username")}
            placeholder="johndoe"
          />
          <FormField
            label="Email"
            required
            error={errors.email}
            registration={register("email")}
            type="email"
            placeholder="john@example.com"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="First Name"
            required
            error={errors.firstName}
            registration={register("firstName")}
            placeholder="John"
          />
          <FormField
            label="Last Name"
            required
            error={errors.lastName}
            registration={register("lastName")}
            placeholder="Doe"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField
            label="Role"
            required
            error={errors.role}
            options={roleOptions}
            value=""
            onValueChange={(v) => setValue("role", v)}
            placeholder="Select role"
          />
          <FormSelectField
            label="Status"
            required
            error={errors.isActive}
            options={statusOptions}
            value=""
            onValueChange={(v) => setValue("isActive", v)}
            placeholder="Select status"
          />
        </div>
      </FormSection>
      <FormActions backHref="/admin" loading={createUser.isPending} submitLabel="Create User" />
    </FormPageLayout>
  )
}
