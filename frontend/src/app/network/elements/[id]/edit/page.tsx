"use client"

import { useParams, useRouter } from "next/navigation"
import { useEffect } from "react"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
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
import { queryKeys } from "@/lib/query-keys"
import { NetworkElementDto } from "@/types/api"

const elementSchema = z.object({
  name: z.string().min(1, "Name is required"),
  elementType: z.string().min(1, "Type is required"),
  vendor: z.string().min(1, "Vendor is required"),
  model: z.string().min(1, "Model is required"),
  ipAddress: z.string().optional(),
  location: z.string().optional(),
  status: z.string().min(1, "Status is required"),
})

type ElementFormData = z.infer<typeof elementSchema>

export default function EditNetworkElementPage() {
  const params = useParams()
  const router = useRouter()
  const queryClient = useQueryClient()
  const id = params.id as string

  const { data: element, isLoading } = useQuery({
    queryKey: queryKeys.networks.elements.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/network/elements/${id}`)
      return res.data as NetworkElementDto
    },
    enabled: !!id,
  })

  const { register, handleSubmit, formState: { errors }, setValue, watch, reset } = useForm<ElementFormData>({
    resolver: zodResolver(elementSchema),
  })

  useEffect(() => {
    if (element) {
      reset({
        name: element.name,
        elementType: element.elementType,
        vendor: element.vendor,
        model: element.model,
        ipAddress: element.ipAddress,
        location: element.location ?? undefined,
        status: element.status,
      })
    }
  }, [element, reset])

  const updateElement = useMutation({
    mutationFn: async (data: ElementFormData) => {
      const res = await api.put(`/api/v1/network/elements/${id}`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.elements.all })
      toast({ title: "Element updated" })
      router.push(`/network/elements/${id}`)
    },
    onError: () => {
      toast({ title: "Failed to update element", variant: "destructive" })
    },
  })

  const onSubmit = (data: ElementFormData) => {
    updateElement.mutate(data)
  }

  if (isLoading) return null

  return (
    <FormPageLayout title="Edit Network Element" backHref={`/network/elements/${id}`} onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Element Details">
        <FormField label="Name" required registration={register("name")} error={errors.name} placeholder="Element name" />
        <FormSelectField
          label="Type"
          required
          value={watch("elementType") || ""}
          onValueChange={(v) => setValue("elementType", v)}
          error={errors.elementType}
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
      <FormActions backHref={`/network/elements/${id}`} loading={updateElement.isPending} />
    </FormPageLayout>
  )
}
