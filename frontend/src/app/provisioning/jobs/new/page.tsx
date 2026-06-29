"use client"

import { useRouter } from "next/navigation"
import { useState } from "react"
import { useQuery, useMutation } from "@tanstack/react-query"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Plus, X } from "lucide-react"
import { ProvisioningTemplateDto } from "@/api/generated"

const jobSchema = z.object({
  name: z.string().min(1, "Name is required"),
  type: z.string().min(1, "Type is required"),
  templateId: z.string().min(1, "Template is required"),
  serviceId: z.string().min(1, "Target service/device is required"),
})

type JobFormData = z.infer<typeof jobSchema>

export default function NewProvisioningJobPage() {
  const router = useRouter()
  const [params, setParams] = useState<{ key: string; value: string }[]>([])

  const { data: templates } = useQuery({
    queryKey: ["provisioning-templates"],
    queryFn: async () => {
      const res = await api.get("/api/v1/provisioning/templates")
      return res.data as ProvisioningTemplateDto[]
    },
  })

  const { register, handleSubmit, formState: { errors }, setValue, watch } = useForm<JobFormData>({
    resolver: zodResolver(jobSchema),
  })

  const createJob = useMutation({
    mutationFn: async (data: JobFormData & { parameters: Record<string, string> }) => {
      const payload = {
        name: data.name,
        type: data.type,
        templateId: data.templateId,
        serviceId: data.serviceId,
        parameters: data.parameters,
        orderId: null,
        orderItemId: null,
        customerId: null,
        serviceType: null,
      }
      const res = await api.post("/api/v1/provisioning/jobs", payload)
      return res.data
    },
    onSuccess: () => {
      toast({ title: "Job created" })
      router.push("/provisioning")
    },
    onError: () => {
      toast({ title: "Failed to create job", variant: "destructive" })
    },
  })

  const onSubmit = (data: JobFormData) => {
    const paramMap: Record<string, string> = {}
    params.forEach((p) => { if (p.key) paramMap[p.key] = p.value })
    createJob.mutate({ ...data, parameters: paramMap })
  }

  const addParam = () => setParams([...params, { key: "", value: "" }])
  const removeParam = (i: number) => setParams(params.filter((_, idx) => idx !== i))
  const updateParam = (i: number, field: "key" | "value", val: string) => {
    const updated = [...params]
    updated[i] = { ...updated[i], [field]: val }
    setParams(updated)
  }

  return (
    <FormPageLayout title="New Provisioning Job" backHref="/provisioning" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Job Details">
        <FormField label="Name" required registration={register("name")} error={errors.name} placeholder="Job name" />
        <FormSelectField
          label="Type"
          required
          value={watch("type") || ""}
          onValueChange={(v) => setValue("type", v)}
          error={errors.type}
          options={[
            { label: "Create", value: "create" },
            { label: "Update", value: "update" },
            { label: "Delete", value: "delete" },
            { label: "Configure", value: "configure" },
          ]}
        />
        <FormSelectField
          label="Template"
          required
          value={watch("templateId") || ""}
          onValueChange={(v) => setValue("templateId", v)}
          error={errors.templateId}
          options={(templates ?? []).map((t) => ({ label: t.name, value: t.id }))}
        />
        <FormField label="Target Service/Device ID" required registration={register("serviceId")} error={errors.serviceId} placeholder="Service or device ID" />
      </FormSection>
      <FormSection title="Parameters">
        {params.map((p, i) => (
          <div key={i} className="flex items-end gap-2">
            <div className="flex-1">
              <FormField label="Key">
                <Input value={p.key} onChange={(e) => updateParam(i, "key", e.target.value)} placeholder="Parameter key" />
              </FormField>
            </div>
            <div className="flex-1">
              <FormField label="Value">
                <Input value={p.value} onChange={(e) => updateParam(i, "value", e.target.value)} placeholder="Parameter value" />
              </FormField>
            </div>
            <Button variant="ghost" size="icon" type="button" onClick={() => removeParam(i)}>
              <X className="h-4 w-4" />
            </Button>
          </div>
        ))}
        <Button variant="outline" size="sm" type="button" onClick={addParam}>
          <Plus className="h-4 w-4 mr-1" /> Add Parameter
        </Button>
      </FormSection>
      <FormActions backHref="/provisioning" loading={createJob.isPending} />
    </FormPageLayout>
  )
}
