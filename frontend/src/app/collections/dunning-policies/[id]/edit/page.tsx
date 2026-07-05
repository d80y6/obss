"use client"

import { useParams, useRouter } from "next/navigation"
import { useForm, useFieldArray } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useEffect } from "react"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import { Button } from "@/components/ui/button"
import { Switch } from "@/components/ui/switch"
import { useDunningPolicy, useUpdateDunningPolicy } from "@/api/hooks/use-collections"
import { LoadingState } from "@/components/shared/LoadingState"
import { ErrorFallback } from "@/components/shared/ErrorFallback"
import { Plus, Trash2 } from "lucide-react"

const dunningFeeSchema = z.object({
  level: z.number().min(1, "Level must be at least 1"),
  fee: z.number().min(0, "Fee must be non-negative"),
})

const schema = z.object({
  name: z.string().min(1, "Name is required").max(200, "Name must be at most 200 characters"),
  description: z.string().optional(),
  isActive: z.boolean(),
  maxDunningLevel: z.number().min(1, "Must be at least 1").max(10, "Must be at most 10"),
  daysBetweenActions: z.number().min(1, "Must be at least 1"),
  escalationAfterDays: z.number().min(1, "Must be at least 1"),
  dunningFeeEntries: z.array(dunningFeeSchema),
})

type FormData = z.infer<typeof schema>

export default function EditDunningPolicyPage() {
  const params = useParams()
  const id = params.id as string
  const router = useRouter()

  const { data: policy, isLoading, error } = useDunningPolicy(id)
  const updateMutation = useUpdateDunningPolicy()

  const { register, handleSubmit, control, formState: { errors }, reset, watch, setValue } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: "",
      description: "",
      isActive: false,
      maxDunningLevel: 3,
      daysBetweenActions: 30,
      escalationAfterDays: 90,
      dunningFeeEntries: [],
    },
  })

  const { fields, append, remove } = useFieldArray({
    control,
    name: "dunningFeeEntries",
  })

  useEffect(() => {
    if (policy) {
      const dunningFeeEntries = Object.entries(policy.dunningFees ?? {})
        .sort(([a], [b]) => Number(a) - Number(b))
        .map(([level, fee]) => ({ level: Number(level), fee }))
      reset({
        name: policy.name,
        description: policy.description ?? "",
        isActive: policy.isActive,
        maxDunningLevel: policy.maxDunningLevel,
        daysBetweenActions: policy.daysBetweenActions,
        escalationAfterDays: policy.escalationAfterDays,
        dunningFeeEntries: dunningFeeEntries.length > 0 ? dunningFeeEntries : [{ level: 1, fee: 0 }],
      })
    }
  }, [policy, reset])

  const onSubmit = (data: FormData) => {
    const dunningFees: Record<number, number> = {}
    for (const entry of data.dunningFeeEntries ?? []) {
      dunningFees[entry.level] = entry.fee
    }
    updateMutation.mutate(
      {
        id,
        name: data.name,
        description: data.description ?? "",
        isActive: data.isActive,
        maxDunningLevel: data.maxDunningLevel,
        daysBetweenActions: data.daysBetweenActions,
        escalationAfterDays: data.escalationAfterDays,
        dunningFees,
      },
      {
        onSuccess: () => {
          toast({ title: "Policy updated" })
          router.push(`/collections/dunning-policies/${id}`)
        },
        onError: () => {
          toast({ title: "Failed to update policy", variant: "destructive" })
        },
      },
    )
  }

  if (isLoading) return <div className="flex-1 p-6"><LoadingState rows={4} /></div>
  if (error || !policy) return <div className="flex-1 p-6"><ErrorFallback message="Failed to load policy" /></div>

  return (
    <FormPageLayout title="Edit Dunning Policy" backHref={`/collections/dunning-policies/${id}`} onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Policy Settings">
        <FormField label="Name" required error={errors.name} registration={register("name")} placeholder="Standard Dunning" />
        <FormField label="Description" error={errors.description} registration={register("description")} placeholder="Optional description" />
        <div className="flex items-center gap-2 pt-2">
          <Switch id="isActive" checked={watch("isActive")} onCheckedChange={(v) => setValue("isActive", v)} />
          <label htmlFor="isActive" className="text-sm font-medium">Active</label>
        </div>
        <div className="grid gap-4 md:grid-cols-3">
          <FormField label="Max Dunning Level" required error={errors.maxDunningLevel} registration={register("maxDunningLevel", { valueAsNumber: true })} type="number" placeholder="3" />
          <FormField label="Days Between Actions" required error={errors.daysBetweenActions} registration={register("daysBetweenActions", { valueAsNumber: true })} type="number" placeholder="30" />
          <FormField label="Escalation After Days" required error={errors.escalationAfterDays} registration={register("escalationAfterDays", { valueAsNumber: true })} type="number" placeholder="90" />
        </div>
      </FormSection>
      <FormSection title="Dunning Fees" description="Set fee amounts for each dunning level.">
        <div className="space-y-3">
          {fields.map((field, index) => (
            <div key={field.id} className="flex items-end gap-3">
              <FormField label="Level" error={errors.dunningFeeEntries?.[index]?.level} registration={register(`dunningFeeEntries.${index}.level`, { valueAsNumber: true })} type="number" placeholder="1" className="w-32" />
              <FormField label="Fee Amount" error={errors.dunningFeeEntries?.[index]?.fee} registration={register(`dunningFeeEntries.${index}.fee`, { valueAsNumber: true })} type="number" placeholder="0" className="flex-1" />
              {fields.length > 1 && (
                <Button variant="ghost" size="icon" type="button" onClick={() => remove(index)} className="mb-2">
                  <Trash2 className="h-4 w-4" />
                </Button>
              )}
            </div>
          ))}
        </div>
        <Button variant="outline" size="sm" type="button" onClick={() => append({ level: fields.length + 1, fee: 0 })} className="mt-3">
          <Plus className="mr-1 h-4 w-4" /> Add Fee Level
        </Button>
      </FormSection>
      <FormActions backHref={`/collections/dunning-policies/${id}`} loading={updateMutation.isPending} submitLabel="Update Policy" />
    </FormPageLayout>
  )
}
