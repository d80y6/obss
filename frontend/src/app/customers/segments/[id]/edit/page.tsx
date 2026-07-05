"use client"

import { useParams, useRouter } from "next/navigation"
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
import { LoadingState } from "@/components/shared/LoadingState"
import { ErrorFallback } from "@/components/shared/ErrorFallback"
import { useSegment } from "@/api/hooks/useSegment"
import api from "@/services/api"
import type { SegmentDto } from "@/api/generated"
import { useEffect } from "react"

const segmentSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
})

type SegmentForm = z.infer<typeof segmentSchema>

export default function EditSegmentPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string

  const { data: segment, isLoading, error } = useSegment(id)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<SegmentForm>({
    resolver: zodResolver(segmentSchema),
  })

  useEffect(() => {
    if (segment) {
      reset({
        name: segment.name,
        description: segment.description ?? "",
      })
    }
  }, [segment, reset])

  const updateMutation = useMutation({
    mutationFn: async (data: SegmentForm) => {
      const res = await api.put<SegmentDto>(`/api/v1/crm/segments/${id}`, data)
      return res.data
    },
    onSuccess: () => {
      toast({ title: "Segment updated", description: "Segment has been updated successfully." })
      router.push(`/customers/segments/${id}`)
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to update segment.", variant: "destructive" })
    },
  })

  const onSubmit = (data: SegmentForm) => {
    updateMutation.mutate(data)
  }

  if (isLoading) return <div className="flex-1 p-6"><LoadingState rows={3} /></div>
  if (error || !segment) return <div className="flex-1 p-6"><ErrorFallback message="Failed to load segment" /></div>

  return (
    <FormPageLayout title="Edit Segment" backHref={`/customers/segments/${id}`} onSubmit={handleSubmit(onSubmit)}>
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
      <FormActions backHref={`/customers/segments/${id}`} loading={updateMutation.isPending} submitLabel="Update Segment" />
    </FormPageLayout>
  )
}
