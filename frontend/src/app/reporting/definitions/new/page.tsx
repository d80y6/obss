"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"

const schema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().min(1, "Description is required"),
  type: z.string().min(1, "Type is required"),
  config: z.string().optional(),
})

type FormData = z.infer<typeof schema>

const typeOptions = [
  { label: "Summary", value: "SUMMARY" },
  { label: "Detailed", value: "DETAILED" },
  { label: "Analytics", value: "ANALYTICS" },
  { label: "Custom", value: "CUSTOM" },
]

export default function NewReportDefinitionPage() {
  const router = useRouter()

  const createMutation = useMutation({
    mutationFn: async (data: FormData) => {
      const config = data.config ? JSON.parse(data.config) : {}
      const res = await api.post("/api/v1/reporting/definitions", {
        name: data.name,
        description: data.description,
        reportType: data.type,
        dataSource: config.dataSource ?? "default",
        query: config.query ?? "",
        outputFormat: config.outputFormat ?? "PDF",
        schedule: null,
      })
      return res.data
    },
    onSuccess: () => {
      toast({ title: "Report created", description: "Report definition has been created." })
      router.push("/reporting/definitions")
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to create report.", variant: "destructive" })
    },
  })

  const { register, handleSubmit, setValue, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const onSubmit = (data: FormData) => createMutation.mutate(data)

  return (
    <FormPageLayout title="New Report Definition" backHref="/reporting/definitions" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Report Information">
        <FormField label="Name" required error={errors.name} registration={register("name")} placeholder="Report name" />
        <FormField label="Description" required error={errors.description} registration={register("description")} placeholder="Report description" />
        <FormSelectField label="Type" required error={errors.type} options={typeOptions} value="" onValueChange={(v) => setValue("type", v)} placeholder="Select type" />
        <FormField label="Config (JSON)" error={errors.config} registration={register("config")} placeholder='{"parameters":[{"key":"dateFrom","label":"From Date","type":"date"}]}' />
      </FormSection>
      <FormActions backHref="/reporting/definitions" loading={createMutation.isPending} submitLabel="Create Report" />
    </FormPageLayout>
  )
}
