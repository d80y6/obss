"use client"

import { useRouter, useParams } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"
import { NotificationTemplateDto } from "@/types/api"

const templateSchema = z.object({
  name: z.string().min(1, "Name is required"),
  type: z.string().min(1, "Type is required"),
  subject: z.string().min(1, "Subject is required"),
  body: z.string().min(1, "Body is required"),
  channel: z.string().min(1, "Channel is required"),
})

type TemplateForm = z.infer<typeof templateSchema>

const typeOptions = [
  { label: "Info", value: "INFO" },
  { label: "Warning", value: "WARNING" },
  { label: "Error", value: "ERROR" },
]

const channelOptions = [
  { label: "Email", value: "EMAIL" },
  { label: "SMS", value: "SMS" },
  { label: "Push", value: "PUSH" },
]

export default function EditNotificationTemplatePage() {
  const router = useRouter()
  const params = useParams()
  const id = params.id as string
  const queryClient = useQueryClient()

  const { data: template, isLoading } = useQuery({
    queryKey: ["notification-templates", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/notifications/templates/${id}`)
      return res.data as NotificationTemplateDto
    },
    enabled: !!id,
  })

  const updateMutation = useMutation({
    mutationFn: async (data: TemplateForm) => {
      const res = await api.put(`/api/v1/notifications/templates/${id}`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["notification-templates"] })
      toast({ title: "Template updated", description: "Notification template has been updated." })
      router.push(`/notifications/templates/${id}`)
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to update template.", variant: "destructive" })
    },
  })

  const { register, handleSubmit, setValue, formState: { errors } } = useForm<TemplateForm>({
    resolver: zodResolver(templateSchema),
    values: template ? {
      name: template.name,
      type: template.variables?.[0] ?? "INFO",
      subject: template.subject,
      body: template.body,
      channel: template.channel,
    } : undefined,
  })

  const onSubmit = (data: TemplateForm) => updateMutation.mutate(data)

  return (
    <FormPageLayout title="Edit Notification Template" backHref={`/notifications/templates/${id}`} onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Template Information">
        <FormField label="Name" required error={errors.name} registration={register("name")} placeholder="Template name" />
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField label="Type" required error={errors.type} options={typeOptions} value={template?.variables?.[0] ?? ""} onValueChange={(v) => setValue("type", v)} placeholder="Select type" />
          <FormSelectField label="Channel" required error={errors.channel} options={channelOptions} value={template?.channel ?? ""} onValueChange={(v) => setValue("channel", v)} placeholder="Select channel" />
        </div>
        <FormField label="Subject" required error={errors.subject} registration={register("subject")} placeholder="Email subject" />
        <FormField label="Body" required error={errors.body} registration={register("body")} placeholder="Template body" />
      </FormSection>
      <FormActions backHref={`/notifications/templates/${id}`} loading={updateMutation.isPending} submitLabel="Update Template" />
    </FormPageLayout>
  )
}
