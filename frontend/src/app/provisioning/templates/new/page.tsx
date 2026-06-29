"use client"

import { useRouter } from "next/navigation"
import { useState } from "react"
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
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Checkbox } from "@/components/ui/checkbox"
import { Plus, X } from "lucide-react"

export default function NewProvisioningTemplatePage() {
  const router = useRouter()

  const { register, handleSubmit, formState: { errors }, setValue, watch } = useForm({
    resolver: zodResolver(z.object({
      name: z.string().min(1, "Name is required"),
      description: z.string().optional(),
      serviceType: z.string().min(1, "Service type is required"),
    })),
  })

  const [parameters, setParameters] = useState<{ name: string; type: string; default: string; required: boolean }[]>([])
  const [steps, setSteps] = useState<{ name: string; type: string; config: string }[]>([])

  const createTemplate = useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const payload = {
        name: data.name,
        description: data.description || "",
        serviceType: data.serviceType,
        action: "Provision",
        workflowDefinitionId: "00000000-0000-0000-0000-000000000000",
      }
      const res = await api.post("/api/v1/provisioning/templates", payload)
      return res.data
    },
    onSuccess: () => {
      toast({ title: "Template created" })
      router.push("/provisioning/templates")
    },
    onError: () => {
      toast({ title: "Failed to create template", variant: "destructive" })
    },
  })

  const onSubmit = (data: Record<string, unknown>) => {
    createTemplate.mutate(data)
  }

  const addParameter = () => setParameters([...parameters, { name: "", type: "string", default: "", required: false }])
  const removeParameter = (i: number) => setParameters(parameters.filter((_, idx) => idx !== i))
  const updateParameter = (i: number, field: string, val: unknown) => {
    const updated = [...parameters]
    updated[i] = { ...updated[i], [field]: val }
    setParameters(updated)
  }

  const addStep = () => setSteps([...steps, { name: "", type: "task", config: "{}" }])
  const removeStep = (i: number) => setSteps(steps.filter((_, idx) => idx !== i))
  const updateStep = (i: number, field: string, val: string) => {
    const updated = [...steps]
    updated[i] = { ...updated[i], [field]: val }
    setSteps(updated)
  }

  return (
    <FormPageLayout title="New Provisioning Template" backHref="/provisioning/templates" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Template Details">
        <FormField label="Name" required registration={register("name")} error={errors.name} placeholder="Template name" />
        <FormField label="Description" registration={register("description")} error={errors.description} placeholder="Template description" />
        <FormSelectField
          label="Service Type"
          required
          value={watch("serviceType") || ""}
          onValueChange={(v) => setValue("serviceType", v)}
          error={errors.serviceType}
          options={[
            { label: "Internet", value: "internet" },
            { label: "VoIP", value: "voip" },
            { label: "IPTV", value: "iptv" },
            { label: "VPN", value: "vpn" },
          ]}
        />
      </FormSection>
      <FormSection title="Parameters">
        {parameters.map((p, i) => (
          <div key={i} className="flex items-end gap-2 flex-wrap">
            <div className="w-40">
              <FormField label="Name">
                <Input value={p.name} onChange={(e) => updateParameter(i, "name", e.target.value)} placeholder="Param name" />
              </FormField>
            </div>
            <div className="w-32">
              <FormSelectField
                label="Type"
                value={p.type}
                onValueChange={(v) => updateParameter(i, "type", v)}
                options={[
                  { label: "String", value: "string" },
                  { label: "Number", value: "number" },
                  { label: "Boolean", value: "boolean" },
                ]}
              />
            </div>
            <div className="w-32">
              <FormField label="Default">
                <Input value={p.default} onChange={(e) => updateParameter(i, "default", e.target.value)} placeholder="Default" />
              </FormField>
            </div>
            <div className="flex items-center gap-2 pb-2">
              <Checkbox checked={p.required} onCheckedChange={(v) => updateParameter(i, "required", v)} />
              <label className="text-sm">Required</label>
            </div>
            <Button variant="ghost" size="icon" type="button" onClick={() => removeParameter(i)}>
              <X className="h-4 w-4" />
            </Button>
          </div>
        ))}
        <Button variant="outline" size="sm" type="button" onClick={addParameter}>
          <Plus className="h-4 w-4 mr-1" /> Add Parameter
        </Button>
      </FormSection>
      <FormSection title="Steps">
        {steps.map((s, i) => (
          <div key={i} className="flex items-end gap-2 flex-wrap">
            <div className="w-48">
              <FormField label="Name">
                <Input value={s.name} onChange={(e) => updateStep(i, "name", e.target.value)} placeholder="Step name" />
              </FormField>
            </div>
            <div className="w-32">
              <FormSelectField
                label="Type"
                value={s.type}
                onValueChange={(v) => updateStep(i, "type", v)}
                options={[
                  { label: "Task", value: "task" },
                  { label: "Approval", value: "approval" },
                  { label: "Notification", value: "notification" },
                ]}
              />
            </div>
            <div className="flex-1">
              <FormField label="Config (JSON)">
                <Input value={s.config} onChange={(e) => updateStep(i, "config", e.target.value)} placeholder='{"key": "value"}' />
              </FormField>
            </div>
            <Button variant="ghost" size="icon" type="button" onClick={() => removeStep(i)}>
              <X className="h-4 w-4" />
            </Button>
          </div>
        ))}
        <Button variant="outline" size="sm" type="button" onClick={addStep}>
          <Plus className="h-4 w-4 mr-1" /> Add Step
        </Button>
      </FormSection>
      <FormActions backHref="/provisioning/templates" loading={createTemplate.isPending} />
    </FormPageLayout>
  )
}
