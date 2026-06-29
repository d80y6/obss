"use client"

import { useParams, useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import { LoadingState } from "@/components/shared/LoadingState"
import { ErrorFallback } from "@/components/shared/ErrorFallback"
import { useUser } from "@/api/hooks/useUser"
import { useUpdateUser } from "@/api/hooks/useUpdateUser"
import { useEffect } from "react"

const userSchema = z.object({
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

export default function EditUserPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string

  const { data: user, isLoading, error } = useUser(id)
  const updateUser = useUpdateUser(id)

  const {
    register,
    handleSubmit,
    setValue,
    reset,
    formState: { errors },
  } = useForm<UserForm>({
    resolver: zodResolver(userSchema),
  })

  useEffect(() => {
    if (user) {
      reset({
        firstName: user.firstName,
        lastName: user.lastName,
        role: user.role ?? "",
        isActive: String(user.isActive),
      })
    }
  }, [user, reset])

  const onSubmit = (data: UserForm) => {
    updateUser.mutate(
      {
        firstName: data.firstName,
        lastName: data.lastName,
      },
      {
        onSuccess: () => {
          toast({ title: "User updated", description: "User has been updated successfully." })
          router.push(`/admin/users/${id}`)
        },
        onError: () => {
          toast({ title: "Error", description: "Failed to update user.", variant: "destructive" })
        },
      }
    )
  }

  if (isLoading) return <div className="flex-1 p-6"><LoadingState rows={3} /></div>
  if (error || !user) return <div className="flex-1 p-6"><ErrorFallback message="Failed to load user" /></div>

  return (
    <FormPageLayout title="Edit User" backHref={`/admin/users/${id}`} onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="User Information">
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
            value={user.role ?? ""}
            onValueChange={(v) => setValue("role", v)}
            placeholder="Select role"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField
            label="Status"
            required
            error={errors.isActive}
            options={statusOptions}
            value={String(user.isActive)}
            onValueChange={(v) => setValue("isActive", v)}
            placeholder="Select status"
          />
        </div>
      </FormSection>
      <FormActions backHref={`/admin/users/${id}`} loading={updateUser.isPending} submitLabel="Update User" />
    </FormPageLayout>
  )
}
