"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"

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

export default function NewNotificationTemplatePage() {
  const router = useRouter()
  const queryClient = useQueryClient()

  const createMutation = useMutation({
    mutationFn: async (data: TemplateForm) => {
      const res = await api.post("/api/v1/notifications/templates", {
        name: data.name,
        notificationType: data.type,
        subject: data.subject,
        body: data.body,
        description: "",
        variables: [],
      })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["notification-templates"] })
      toast({ title: "Template created", description: "Notification template has been created." })
      router.push("/notifications/templates")
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to create template.", variant: "destructive" })
    },
  })

  const { register, handleSubmit, setValue, formState: { errors } } = useForm<TemplateForm>({
    resolver: zodResolver(templateSchema),
  })

  const onSubmit = (data: TemplateForm) => createMutation.mutate(data)

  return (
    <FormPageLayout title="New Notification Template" backHref="/notifications/templates" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Template Information">
        <FormField label="Name" required error={errors.name} registration={register("name")} placeholder="Template name" />
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField label="Type" required error={errors.type} options={typeOptions} value="" onValueChange={(v) => setValue("type", v)} placeholder="Select type" />
          <FormSelectField label="Channel" required error={errors.channel} options={channelOptions} value="" onValueChange={(v) => setValue("channel", v)} placeholder="Select channel" />
        </div>
        <FormField label="Subject" required error={errors.subject} registration={register("subject")} placeholder="Email subject (use {{variable}} for placeholders)" />
        <FormField label="Body" required error={errors.body} registration={register("body")} placeholder="Template body with {{variable}} placeholders" />
      </FormSection>
      <FormActions backHref="/notifications/templates" loading={createMutation.isPending} submitLabel="Create Template" />
    </FormPageLayout>
  )
}
