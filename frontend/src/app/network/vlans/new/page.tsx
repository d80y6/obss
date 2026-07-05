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
import { useCreateVlan } from "@/api/hooks/useCreateVlan"

const vlanSchema = z.object({
  vlanId: z.string().min(1, "VLAN ID is required"),
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
  subnet: z.string().optional(),
  status: z.string().min(1, "Status is required"),
})

type VlanForm = z.input<typeof vlanSchema>

const statusOptions = [
  { label: "Active", value: "active" },
  { label: "Inactive", value: "inactive" },
  { label: "Pending", value: "pending" },
]

export default function NewVlanPage() {
  const router = useRouter()
  const createVlan = useCreateVlan()

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<VlanForm>({
    resolver: zodResolver(vlanSchema),
    defaultValues: { status: "active" },
  })

  const onSubmit = (data: VlanForm) => {
    createVlan.mutate({
      vlanId: parseInt(data.vlanId, 10),
      name: data.name,
      description: data.description ?? null,
      location: null,
      tenantId: "",
    } as Parameters<typeof createVlan.mutate>[0], {
      onSuccess: () => {
        toast({ title: "VLAN created", description: `${data.name} has been created.` })
        router.push("/network/vlans")
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to create VLAN.", variant: "destructive" })
      },
    })
  }

  return (
    <FormPageLayout title="New VLAN" backHref="/network/vlans" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="VLAN Details">
        <FormField label="VLAN ID" required registration={register("vlanId")} error={errors.vlanId} type="number" placeholder="1-4094" />
        <FormField label="Name" required registration={register("name")} error={errors.name} placeholder="VLAN name" />
        <FormField label="Description" registration={register("description")} error={errors.description} placeholder="VLAN description" />
        <FormField label="Subnet" registration={register("subnet")} error={errors.subnet} placeholder="e.g. 10.0.1.0/24" />
        <FormSelectField
          label="Status"
          required
          error={errors.status}
          options={statusOptions}
          value="active"
          onValueChange={(v) => setValue("status", v)}
          placeholder="Select status"
        />
      </FormSection>
      <FormActions backHref="/network/vlans" loading={createVlan.isPending} submitLabel="Create VLAN" />
    </FormPageLayout>
  )
}
