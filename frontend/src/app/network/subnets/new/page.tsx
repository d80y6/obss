"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import { useCreateSubnet } from "@/api/hooks/useSubnets"

const subnetSchema = z.object({
  name: z.string().min(1, "Name is required"),
  network: z.string().min(1, "Network is required"),
  gateway: z.string().optional(),
  vlanId: z.string().min(1, "VLAN ID is required"),
  location: z.string().optional(),
  description: z.string().optional(),
})

type SubnetForm = z.input<typeof subnetSchema>

export default function NewSubnetPage() {
  const router = useRouter()
  const createSubnet = useCreateSubnet()

  const { register, handleSubmit, formState: { errors } } = useForm<SubnetForm>({
    resolver: zodResolver(subnetSchema),
  })

  const onSubmit = (data: SubnetForm) => {
    createSubnet.mutate({
      name: data.name,
      network: data.network,
      gateway: data.gateway || null,
      vlanId: parseInt(data.vlanId, 10),
      location: data.location || null,
      description: data.description || null,
    } as Parameters<typeof createSubnet.mutate>[0], {
      onSuccess: () => {
        toast({ title: "Subnet created" })
        router.push("/network/subnets")
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to create subnet.", variant: "destructive" })
      },
    })
  }

  return (
    <FormPageLayout title="New Subnet" backHref="/network/subnets" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Subnet Details">
        <FormField label="Name" required registration={register("name")} error={errors.name} placeholder="Subnet name" />
        <FormField label="Network (CIDR)" required registration={register("network")} error={errors.network} placeholder="10.0.0.0/24" />
        <FormField label="VLAN ID" required registration={register("vlanId")} error={errors.vlanId} type="number" placeholder="100" />
        <FormField label="Gateway" registration={register("gateway")} error={errors.gateway} placeholder="10.0.0.1" />
        <FormField label="Location" registration={register("location")} error={errors.location} placeholder="Data center" />
        <FormField label="Description" registration={register("description")} error={errors.description} placeholder="Optional description" />
      </FormSection>
      <FormActions backHref="/network/subnets" loading={createSubnet.isPending} submitLabel="Create Subnet" />
    </FormPageLayout>
  )
}
