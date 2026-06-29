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

const vlanSchema = z.object({
  vlanId: z.string().min(1, "VLAN ID is required"),
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
  subnet: z.string().optional(),
})

type VlanFormData = z.infer<typeof vlanSchema>

export default function NewVlanPage() {
  const router = useRouter()

  const { register, handleSubmit, formState: { errors } } = useForm<VlanFormData>({
    resolver: zodResolver(vlanSchema),
  })

  const createVlan = useMutation({
    mutationFn: async (data: VlanFormData) => {
      const payload = {
        vlanId: parseInt(data.vlanId, 10),
        name: data.name,
        description: data.description || "",
        subnet: data.subnet || "",
        location: "",
        status: "active",
      }
      const res = await api.post("/api/v1/network/vlans", payload)
      return res.data
    },
    onSuccess: () => {
      toast({ title: "VLAN created" })
      router.push("/network/vlans")
    },
    onError: () => {
      toast({ title: "Failed to create VLAN", variant: "destructive" })
    },
  })

  const onSubmit = (data: VlanFormData) => {
    createVlan.mutate(data)
  }

  return (
    <FormPageLayout title="New VLAN" backHref="/network/vlans" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="VLAN Details">
        <FormField label="VLAN ID" required registration={register("vlanId")} error={errors.vlanId} type="number" placeholder="1-4094" />
        <FormField label="Name" required registration={register("name")} error={errors.name} placeholder="VLAN name" />
        <FormField label="Description" registration={register("description")} error={errors.description} placeholder="VLAN description" />
        <FormField label="Subnet" registration={register("subnet")} error={errors.subnet} placeholder="e.g. 10.0.1.0/24" />
      </FormSection>
      <FormActions backHref="/network/vlans" loading={createVlan.isPending} />
    </FormPageLayout>
  )
}
