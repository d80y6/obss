"use client"

import { useRouter } from "next/navigation"
import { useState } from "react"
import { useQuery } from "@tanstack/react-query"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import { api } from "@/api/client"
import { useCreateService } from "@/api/hooks/use-service-inventory"
import type { CustomerDto, SubscriptionDto } from "@/api/generated"
import { Button } from "@/components/ui/button"
import { Plus, X } from "lucide-react"
import { Input } from "@/components/ui/input"

const serviceSchema = z.object({
  serviceIdentifier: z.string().min(1, "Service identifier is required"),
  serviceType: z.string().min(1, "Type is required"),
  customerId: z.string().min(1, "Customer is required"),
  subscriptionId: z.string().min(1, "Subscription is required"),
})

type ServiceFormData = z.infer<typeof serviceSchema>

export default function NewServicePage() {
  const router = useRouter()
  const [attributes, setAttributes] = useState<{ key: string; value: string }[]>([])

  const { data: customers } = useQuery({
    queryKey: ["customers"],
    queryFn: async () => {
      const res = await api.get("/api/v1/crm/customers")
      return res.data as CustomerDto[]
    },
  })

  const { data: subscriptions } = useQuery({
    queryKey: ["subscriptions"],
    queryFn: async () => {
      const res = await api.get("/api/v1/subscriptions/subscriptions")
      return res.data as SubscriptionDto[]
    },
  })

  const { register, handleSubmit, formState: { errors }, setValue, watch } = useForm<ServiceFormData>({
    resolver: zodResolver(serviceSchema),
  })

  const [selectedCustomerId, selectedServiceType] = watch(["customerId", "serviceType"])

  const createService = useCreateService()

  const onSubmit = (data: ServiceFormData) => {
    const attrMap: Record<string, string> = {}
    attributes.forEach((a) => { if (a.key) attrMap[a.key] = a.value })
    createService.mutate({
      customerId: data.customerId,
      subscriptionId: data.subscriptionId,
      serviceType: data.serviceType,
      serviceIdentifier: data.serviceIdentifier,
      location: "",
      configuration: JSON.stringify(attrMap),
    }, {
      onSuccess: () => {
        toast({ title: "Service created" })
        router.push("/service-inventory")
      },
      onError: () => {
        toast({ title: "Failed to create service", variant: "destructive" })
      },
    })
  }

  const addAttribute = () => setAttributes([...attributes, { key: "", value: "" }])
  const removeAttribute = (i: number) => setAttributes(attributes.filter((_, idx) => idx !== i))
  const updateAttribute = (i: number, field: "key" | "value", val: string) => {
    const updated = [...attributes]
    updated[i] = { ...updated[i], [field]: val }
    setAttributes(updated)
  }

  return (
    <FormPageLayout title="New Service" backHref="/service-inventory" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Service Details">
        <FormField label="Service Identifier" required registration={register("serviceIdentifier")} error={errors.serviceIdentifier} placeholder="e.g. SVC-001" />
        <FormSelectField
          label="Type"
          required
          value={selectedServiceType || ""}
          onValueChange={(v) => setValue("serviceType", v)}
          error={errors.serviceType}
          options={[
            { label: "FTTH", value: "FTTH" },
            { label: "ADSL", value: "ADSL" },
            { label: "Fixed Wireless", value: "FixedWireless" },
            { label: "Yemen WiFi", value: "YemenWiFi" },
            { label: "VoIP", value: "VoIP" },
            { label: "Static IP", value: "StaticIP" },
            { label: "DIA", value: "DIA" },
            { label: "Ethernet", value: "Ethernet" },
            { label: "PRI", value: "PRI" },
            { label: "VPS", value: "VPS" },
            { label: "Dedicated Server", value: "DedicatedServer" },
            { label: "Colocation", value: "Colocation" },
            { label: "Hosting", value: "Hosting" },
            { label: "Domain", value: "Domain" },
          ]}
        />
        <FormSelectField
          label="Customer"
          required
          value={selectedCustomerId || ""}
          onValueChange={(v) => setValue("customerId", v)}
          error={errors.customerId}
          options={(customers ?? []).map((c) => ({ label: `${c.displayName}`, value: c.id }))}
        />
        <FormSelectField
          label="Subscription"
          required
          value={watch("subscriptionId") || ""}
          onValueChange={(v) => setValue("subscriptionId", v)}
          error={errors.subscriptionId}
          options={(subscriptions ?? []).map((s) => ({ label: s.offerName, value: s.id }))}
        />
      </FormSection>
      <FormSection title="Attributes">
        {attributes.map((attr, i) => (
          <div key={i} className="flex items-end gap-2">
            <div className="flex-1">
              <FormField label="Key">
                <Input value={attr.key} onChange={(e) => updateAttribute(i, "key", e.target.value)} placeholder="Attribute key" />
              </FormField>
            </div>
            <div className="flex-1">
              <FormField label="Value">
                <Input value={attr.value} onChange={(e) => updateAttribute(i, "value", e.target.value)} placeholder="Attribute value" />
              </FormField>
            </div>
            <Button variant="ghost" size="icon" type="button" onClick={() => removeAttribute(i)}>
              <X className="h-4 w-4" />
            </Button>
          </div>
        ))}
        <Button variant="outline" size="sm" type="button" onClick={addAttribute}>
          <Plus className="h-4 w-4 mr-1" /> Add Attribute
        </Button>
      </FormSection>
      <FormActions backHref="/service-inventory" loading={createService.isPending} />
    </FormPageLayout>
  )
}
