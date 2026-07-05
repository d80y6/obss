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
import { useCreateOlt } from "@/api/hooks/useCreateOlt"

const oltSchema = z.object({
  name: z.string().min(1, "Name is required"),
  hostname: z.string().min(1, "Hostname is required"),
  ipAddress: z.string().min(1, "IP address is required"),
  vendor: z.string().min(1, "Vendor is required"),
  model: z.string().min(1, "Model is required"),
  location: z.string().optional(),
  softwareVersion: z.string().optional(),
  maxPONPorts: z.string().min(1, "Max PON ports is required"),
  maxONTPerPort: z.string().min(1, "Max ONT per port is required"),
  maxBandwidth: z.string().min(1, "Max bandwidth is required"),
})

type OltForm = z.input<typeof oltSchema>

const vendorOptions = [
  { label: "Huawei", value: "Huawei" },
  { label: "ZTE", value: "ZTE" },
  { label: "Nokia", value: "Nokia" },
  { label: "Cisco", value: "Cisco" },
  { label: "Calix", value: "Calix" },
  { label: "ADTRAN", value: "ADTRAN" },
]

export default function NewOltPage() {
  const router = useRouter()
  const createOlt = useCreateOlt()

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<OltForm>({
    resolver: zodResolver(oltSchema),
  })

  const onSubmit = (data: OltForm) => {
    createOlt.mutate({
      name: data.name,
      hostname: data.hostname,
      ipAddress: data.ipAddress,
      vendor: data.vendor,
      model: data.model,
      location: data.location ?? null,
      softwareVersion: data.softwareVersion ?? null,
      maxPONPorts: parseInt(data.maxPONPorts, 10),
      maxONTPerPort: parseInt(data.maxONTPerPort, 10),
      maxBandwidth: parseInt(data.maxBandwidth, 10),
    } as Parameters<typeof createOlt.mutate>[0], {
      onSuccess: () => {
        toast({ title: "OLT created", description: `${data.name} has been created.` })
        router.push("/network/olts")
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to create OLT.", variant: "destructive" })
      },
    })
  }

  return (
    <FormPageLayout title="New OLT" backHref="/network/olts" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="OLT Details">
        <FormField label="Name" required registration={register("name")} error={errors.name} placeholder="OLT name" />
        <FormField label="Hostname" required registration={register("hostname")} error={errors.hostname} placeholder="Hostname" />
        <FormField label="IP Address" required registration={register("ipAddress")} error={errors.ipAddress} placeholder="10.0.0.1" />
        <FormSelectField
          label="Vendor"
          required
          error={errors.vendor}
          options={vendorOptions}
          value=""
          onValueChange={(v) => setValue("vendor", v)}
          placeholder="Select vendor"
        />
        <FormField label="Model" required registration={register("model")} error={errors.model} placeholder="e.g. MA5800-X17" />
        <FormField label="Location" registration={register("location")} error={errors.location} placeholder="Central office" />
        <FormField label="Software Version" registration={register("softwareVersion")} error={errors.softwareVersion} placeholder="V1.0" />
        <FormField label="Max PON Ports" required registration={register("maxPONPorts")} error={errors.maxPONPorts} type="number" placeholder="16" />
        <FormField label="Max ONT Per Port" required registration={register("maxONTPerPort")} error={errors.maxONTPerPort} type="number" placeholder="64" />
        <FormField label="Max Bandwidth (Gbps)" required registration={register("maxBandwidth")} error={errors.maxBandwidth} type="number" placeholder="10" />
      </FormSection>
      <FormActions backHref="/network/olts" loading={createOlt.isPending} submitLabel="Create OLT" />
    </FormPageLayout>
  )
}
