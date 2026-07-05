"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation, useQuery } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"
import type { WorkflowDefinitionDto } from "@/api/generated"

const slaSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
  definitionId: z.string().min(1, "Definition is required"),
  responseTime: z.coerce.number().min(1, "Response time must be >= 1"),
  resolutionTime: z.coerce.number().min(1, "Resolution time must be >= 1"),
})

type SlaFormData = z.output<typeof slaSchema>

export default function NewWorkflowSlaPage() {
  const router = useRouter()

  const { data: definitions } = useQuery({
    queryKey: ["workflow-definitions"],
    queryFn: async () => {
      const res = await api.get("/api/v1/workflow/definitions")
      return res.data as WorkflowDefinitionDto[]
    },
  })

  const { register, handleSubmit, formState: { errors }, setValue, watch } = useForm<SlaFormData>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(slaSchema) as any,
  })

  const createSla = useMutation({
    mutationFn: async (data: SlaFormData) => {
      const res = await api.post("/api/v1/workflow/slas", data)
      return res.data
    },
    onSuccess: () => {
      toast({ title: "SLA definition created" })
      router.push("/workflow/slas")
    },
    onError: () => {
      toast({ title: "Failed to create SLA", variant: "destructive" })
    },
  })

  const onSubmit = (data: SlaFormData) => {
    createSla.mutate(data)
  }

  return (
    <FormPageLayout title="New SLA Definition" backHref="/workflow/slas" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="SLA Details">
        <FormField label="Name" required registration={register("name")} error={errors.name} placeholder="SLA name" />
        <FormField label="Description" registration={register("description")} error={errors.description} placeholder="SLA description" />
        <FormSelectField
          label="Workflow Definition"
          required
          value={watch("definitionId") || ""}
          onValueChange={(v) => setValue("definitionId", v)}
          error={errors.definitionId}
          options={(definitions ?? []).map((d) => ({ label: `${d.name} v${d.version}`, value: d.id }))}
        />
        <FormField label="Response Time (minutes)" required registration={register("responseTime")} error={errors.responseTime} type="number" placeholder="30" />
        <FormField label="Resolution Time (minutes)" required registration={register("resolutionTime")} error={errors.resolutionTime} type="number" placeholder="120" />
      </FormSection>
      <FormActions backHref="/workflow/slas" loading={createSla.isPending} />
    </FormPageLayout>
  )
}
