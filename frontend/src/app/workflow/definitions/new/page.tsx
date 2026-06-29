"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"

const defSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
  category: z.string().optional(),
  version: z.coerce.number().min(1, "Version must be >= 1"),
})

interface DefFormData {
  name: string
  description?: string
  category?: string
  version: number
}

export default function NewWorkflowDefinitionPage() {
  const router = useRouter()

  const { register, handleSubmit, formState: { errors } } = useForm<DefFormData>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(defSchema) as any,
    defaultValues: { version: 1, name: "" },
  })

  const createDef = useMutation({
    mutationFn: async (data: DefFormData) => {
      const res = await api.post("/api/v1/workflow/definitions", {
        name: data.name,
        description: data.description,
        category: data.category || "",
      })
      return res.data
    },
    onSuccess: () => {
      toast({ title: "Workflow definition created" })
      router.push("/workflow")
    },
    onError: () => {
      toast({ title: "Failed to create definition", variant: "destructive" })
    },
  })

  const onSubmit = (data: DefFormData) => {
    createDef.mutate(data)
  }

  return (
    <FormPageLayout title="New Workflow Definition" backHref="/workflow" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Definition Details">
        <FormField label="Name" required registration={register("name")} error={errors.name} placeholder="Definition name" />
        <FormField label="Description" registration={register("description")} error={errors.description} placeholder="Definition description" />
        <FormField label="Category" registration={register("category")} error={errors.category} placeholder="e.g. provisioning, maintenance" />
        <FormField label="Version" required registration={register("version")} error={errors.version} type="number" placeholder="1" />
      </FormSection>
      <FormActions backHref="/workflow" loading={createDef.isPending} />
    </FormPageLayout>
  )
}
