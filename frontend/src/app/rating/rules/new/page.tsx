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
import { useCreateRatingRule } from "@/api/hooks/use-rating"
import type { CreateRatingRuleCommand } from "@/api/generated"

const ruleSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
  ruleType: z.string().min(1, "Rule type is required"),
  priority: z.coerce.number().int().min(0, "Priority must be a positive number"),
})

type RuleForm = z.infer<typeof ruleSchema>

const ruleTypes = [
  { label: "Flat Rate", value: "FLAT_RATE" },
  { label: "Tiered", value: "TIERED" },
  { label: "Volume", value: "VOLUME" },
  { label: "Time Based", value: "TIME_BASED" },
]

export default function NewRatingRulePage() {
  const router = useRouter()
  const createMutation = useCreateRatingRule()

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<RuleForm>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(ruleSchema) as any,
  })

  const onSubmit = (data: RuleForm) => {
    const command: CreateRatingRuleCommand = {
      name: data.name,
      description: data.description ?? null,
      ruleType: data.ruleType,
      productId: null,
      offerId: null,
      priority: data.priority,
      tiers: [],
    }
    createMutation.mutate(command, {
      onSuccess: () => {
        toast({ title: "Rule created", description: "Rating rule has been created successfully." })
        router.push("/rating/rules")
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to create rating rule.", variant: "destructive" })
      },
    })
  }

  return (
    <FormPageLayout title="New Rating Rule" backHref="/rating/rules" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Rule Information">
        <FormField
          label="Name"
          required
          error={errors.name}
          registration={register("name")}
          placeholder="Rule name"
        />
        <FormField
          label="Description"
          error={errors.description}
          registration={register("description")}
          placeholder="Rule description"
        />
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField
            label="Rule Type"
            required
            error={errors.ruleType}
            options={ruleTypes}
            value=""
            onValueChange={(v) => setValue("ruleType", v)}
            placeholder="Select rule type"
          />
          <FormField
            label="Priority"
            required
            type="number"
            error={errors.priority}
            registration={register("priority")}
            placeholder="0"
          />
        </div>
      </FormSection>
      <FormActions backHref="/rating/rules" loading={createMutation.isPending} submitLabel="Create Rule" />
    </FormPageLayout>
  )
}
