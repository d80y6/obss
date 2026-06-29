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
import { SegmentDto } from "@/types/api"

const segmentSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
})

type SegmentForm = z.infer<typeof segmentSchema>

export default function NewSegmentPage() {
  const router = useRouter()

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<SegmentForm>({
    resolver: zodResolver(segmentSchema),
  })

  const createMutation = useMutation({
    mutationFn: async (data: SegmentForm) => {
      const res = await api.post<SegmentDto>("/api/v1/crm/segments", data)
      return res.data
    },
    onSuccess: (data) => {
      toast({ title: "Segment created", description: `${data.name} has been created successfully.` })
      router.push(`/customers/segments/${data.id}`)
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to create segment.", variant: "destructive" })
    },
  })

  const onSubmit = (data: SegmentForm) => {
    createMutation.mutate(data)
  }

  return (
    <FormPageLayout title="New Segment" backHref="/customers/segments" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Segment Information">
        <FormField
          label="Name"
          required
          error={errors.name}
          registration={register("name")}
          placeholder="Segment name"
        />
        <FormField
          label="Description"
          error={errors.description}
          registration={register("description")}
          placeholder="Segment description"
        />
      </FormSection>
      <FormActions backHref="/customers/segments" loading={createMutation.isPending} submitLabel="Create Segment" />
    </FormPageLayout>
  )
}
