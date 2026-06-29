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

const elementSchema = z.object({
  name: z.string().min(1, "Name is required"),
  type: z.string().min(1, "Type is required"),
  vendor: z.string().min(1, "Vendor is required"),
  model: z.string().min(1, "Model is required"),
  ipAddress: z.string().optional(),
  location: z.string().optional(),
  status: z.string().min(1, "Status is required"),
})

type ElementFormData = z.infer<typeof elementSchema>

export default function NewNetworkElementPage() {
  const router = useRouter()

  const { register, handleSubmit, formState: { errors }, setValue, watch } = useForm<ElementFormData>({
    resolver: zodResolver(elementSchema),
    defaultValues: { status: "active" },
  })

  const createElement = useMutation({
    mutationFn: async (data: ElementFormData) => {
      const payload = {
        name: data.name,
        elementType: data.type,
        vendor: data.vendor,
        model: data.model,
        ipAddress: data.ipAddress || "",
        location: data.location || "",
        status: data.status,
        softwareVersion: "",
        serialNumber: "",
      }
      const res = await api.post("/api/v1/network/elements", payload)
      return res.data
    },
    onSuccess: () => {
      toast({ title: "Network element created" })
      router.push("/network/elements")
    },
    onError: () => {
      toast({ title: "Failed to create element", variant: "destructive" })
    },
  })

  const onSubmit = (data: ElementFormData) => {
    createElement.mutate(data)
  }

  return (
    <FormPageLayout title="New Network Element" backHref="/network/elements" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Element Details">
        <FormField label="Name" required registration={register("name")} error={errors.name} placeholder="Element name" />
        <FormSelectField
          label="Type"
          required
          value={watch("type") || ""}
          onValueChange={(v) => setValue("type", v)}
          error={errors.type}
          options={[
            { label: "OLT", value: "OLT" },
            { label: "ONT", value: "ONT" },
            { label: "Switch", value: "switch" },
            { label: "Router", value: "router" },
            { label: "Firewall", value: "firewall" },
          ]}
        />
        <FormField label="Vendor" required registration={register("vendor")} error={errors.vendor} placeholder="e.g. Cisco, Huawei" />
        <FormField label="Model" required registration={register("model")} error={errors.model} placeholder="e.g. ASR-9000" />
        <FormField label="IP Address" registration={register("ipAddress")} error={errors.ipAddress} placeholder="e.g. 10.0.0.1" />
        <FormField label="Location" registration={register("location")} error={errors.location} placeholder="e.g. Data Center A" />
        <FormSelectField
          label="Status"
          required
          value={watch("status") || "active"}
          onValueChange={(v) => setValue("status", v)}
          error={errors.status}
          options={[
            { label: "Active", value: "active" },
            { label: "Inactive", value: "inactive" },
            { label: "Maintenance", value: "maintenance" },
          ]}
        />
      </FormSection>
      <FormActions backHref="/network/elements" loading={createElement.isPending} />
    </FormPageLayout>
  )
}
