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
import { useCreateApiRoute } from "@/api/hooks/use-api-gateway"

const schema = z.object({
  name: z.string().min(1, "Name is required"),
  path: z.string().min(1, "Path is required"),
  method: z.string().min(1, "Method is required"),
  upstreamUrl: z.string().min(1, "Upstream URL is required"),
  authRequired: z.string().optional(),
  rateLimit: z.string().optional(),
})

type FormData = z.infer<typeof schema>

const methodOptions = [
  { label: "GET", value: "GET" },
  { label: "POST", value: "POST" },
  { label: "PUT", value: "PUT" },
  { label: "DELETE", value: "DELETE" },
  { label: "PATCH", value: "PATCH" },
]

export default function NewApiRoutePage() {
  const router = useRouter()
  const createMutation = useCreateApiRoute()

  const { register, handleSubmit, setValue, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const onSubmit = (data: FormData) => {
    createMutation.mutate({
      path: data.path,
      method: data.method,
      targetModule: data.upstreamUrl?.split("/")[0] || "",
      targetPath: data.upstreamUrl || "",
      requireAuthentication: data.authRequired === "true",
      rateLimitPerMinute: parseInt(data.rateLimit || "0") || 0,
      requiredPermissions: [],
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any, {
      onSuccess: () => {
        toast({ title: "Route created", description: "API route has been created." })
        router.push("/api-gateway/routes")
      },
      onError: () => toast({ title: "Error", description: "Failed to create route.", variant: "destructive" }),
    })
  }

  return (
    <FormPageLayout title="New API Route" backHref="/api-gateway/routes" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Route Configuration">
        <FormField label="Name" required error={errors.name} registration={register("name")} placeholder="Route name" />
        <div className="grid gap-4 md:grid-cols-2">
          <FormField label="Path" required error={errors.path} registration={register("path")} placeholder="/api/v1/users" />
          <FormSelectField label="Method" required error={errors.method} options={methodOptions} value="" onValueChange={(v) => setValue("method", v)} placeholder="Select method" />
        </div>
        <FormField label="Upstream URL" required error={errors.upstreamUrl} registration={register("upstreamUrl")} placeholder="https://backend-service:8080" />
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField label="Auth Required" options={[{ label: "Yes", value: "true" }, { label: "No", value: "false" }]} value="" onValueChange={(v) => setValue("authRequired", v)} placeholder="Auth required?" />
          <FormField label="Rate Limit (req/s)" error={errors.rateLimit} registration={register("rateLimit")} type="number" placeholder="100" />
        </div>
      </FormSection>
      <FormActions backHref="/api-gateway/routes" loading={createMutation.isPending} submitLabel="Create Route" />
    </FormPageLayout>
  )
}
