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
import { useCreateNetworkElement } from "@/api/hooks/useCreateNetworkElement"
import type { CreateNetworkElementCommand } from "@/api/generated"

const elementSchema = z.object({
  name: z.string().min(1, "Name is required"),
  hostname: z.string().min(1, "Hostname is required"),
  ipAddress: z.string().optional(),
  elementType: z.string().min(1, "Type is required"),
  vendor: z.string().min(1, "Vendor is required"),
  model: z.string().min(1, "Model is required"),
})

type ElementFormData = z.input<typeof elementSchema>

const elementTypes = [
  { label: "OLT", value: "OLT" },
  { label: "ONT", value: "ONT" },
  { label: "Switch", value: "switch" },
  { label: "Router", value: "router" },
  { label: "Firewall", value: "firewall" },
]

export default function NewNetworkElementPage() {
  const router = useRouter()
  const createNetworkElement = useCreateNetworkElement()

  const { register, handleSubmit, setValue, formState: { errors } } = useForm<ElementFormData>({
    resolver: zodResolver(elementSchema),
  })

  const onSubmit = (data: ElementFormData) => {
    createNetworkElement.mutate({
      tenantId: "00000000-0000-0000-0000-000000000000",
      name: data.name,
      hostname: data.hostname,
      ipAddress: data.ipAddress || "",
      elementType: data.elementType,
      vendor: data.vendor,
      model: data.model,
      softwareVersion: null,
      serialNumber: null,
      location: null,
      managementIP: null,
      snmpCommunity: null,
      isManaged: true,
    } as CreateNetworkElementCommand, {
      onSuccess: (element) => {
        toast({ title: "Network element created", description: `${element.name} has been created successfully.` })
        router.push("/network/elements")
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to create network element.", variant: "destructive" })
      },
    })
  }

  return (
    <FormPageLayout title="New Network Element" backHref="/network/elements" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Element Details">
        <FormField label="Name" required registration={register("name")} error={errors.name} placeholder="Element name" />
        <FormField label="Hostname" required registration={register("hostname")} error={errors.hostname} placeholder="e.g. olt-01.example.com" />
        <FormField label="IP Address" registration={register("ipAddress")} error={errors.ipAddress} placeholder="e.g. 10.0.0.1" />
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField
            label="Type"
            required
            error={errors.elementType}
            options={elementTypes}
            value=""
            onValueChange={(v) => setValue("elementType", v)}
            placeholder="Select type"
          />
          <FormField label="Vendor" required registration={register("vendor")} error={errors.vendor} placeholder="e.g. Cisco, Huawei" />
        </div>
        <FormField label="Model" required registration={register("model")} error={errors.model} placeholder="e.g. ASR-9000" />
      </FormSection>
      <FormActions backHref="/network/elements" loading={createNetworkElement.isPending} submitLabel="Create Network Element" />
    </FormPageLayout>
  )
}
