"use client"

import { useRouter } from "next/navigation"
import { PageHeader } from "@/components/shared/PageHeader"
import { useNasDevice } from "@/api/hooks/useNasDevice"
import { useUpdateNas } from "@/api/hooks/useUpdateNas"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Card, CardContent } from "@/components/ui/card"
import { toast } from "@/components/ui/toast"
import { useEffect } from "react"

const nasSchema = z.object({
  name: z.string().min(1, "Name is required"),
  nasIpAddress: z.string().min(1, "IP address is required"),
  nasType: z.enum(["cisco", "huawei", "mikrotik", "linux", "other"]),
  location: z.string().optional(),
})

type NasFormValues = z.infer<typeof nasSchema>

export default function EditNasPage({ params }: { params: { id: string } }) {
  const router = useRouter()
  const { data: nas, isLoading } = useNasDevice(params.id)
  const updateNas = useUpdateNas()

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<NasFormValues>({
    resolver: zodResolver(nasSchema),
  })

  useEffect(() => {
    if (nas) {
      reset({
        name: nas.name,
        nasIpAddress: nas.nasIpAddress,
        nasType: nas.nasType as NasFormValues["nasType"],
        location: nas.location ?? "",
      })
    }
  }, [nas, reset])

  const onSubmit = async (values: NasFormValues) => {
    await updateNas.mutateAsync({ id: params.id, ...values })
    toast({ title: "NAS updated", description: `${values.name} has been updated.` })
    router.push(`/aaa/nas/${params.id}`)
  }

  if (isLoading) return null

  return (
    <div className="space-y-4">
      <PageHeader title={`Edit ${nas?.name ?? "NAS"}`} backHref={`/aaa/nas/${params.id}`} />
      <Card>
        <CardContent className="pt-6">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 max-w-lg">
            <div className="space-y-2">
              <Label htmlFor="name">Name</Label>
              <Input id="name" {...register("name")} />
              {errors.name && <p className="text-sm text-destructive">{errors.name.message}</p>}
            </div>
            <div className="space-y-2">
              <Label htmlFor="nasIpAddress">IP Address</Label>
              <Input id="nasIpAddress" {...register("nasIpAddress")} />
              {errors.nasIpAddress && <p className="text-sm text-destructive">{errors.nasIpAddress.message}</p>}
            </div>
            <div className="space-y-2">
              <Label htmlFor="nasType">Type</Label>
              <Select value={watch("nasType")} onValueChange={(v) => setValue("nasType", v as NasFormValues["nasType"])}>
                <SelectTrigger><SelectValue placeholder="Select type" /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="cisco">Cisco</SelectItem>
                  <SelectItem value="huawei">Huawei</SelectItem>
                  <SelectItem value="mikrotik">MikroTik</SelectItem>
                  <SelectItem value="linux">Linux</SelectItem>
                  <SelectItem value="other">Other</SelectItem>
                </SelectContent>
              </Select>
              {errors.nasType && <p className="text-sm text-destructive">{errors.nasType.message}</p>}
            </div>
            <div className="space-y-2">
              <Label htmlFor="location">Location (optional)</Label>
              <Input id="location" {...register("location")} />
            </div>
            <div className="flex gap-2 pt-2">
              <Button type="submit" disabled={isSubmitting}>Save Changes</Button>
              <Button type="button" variant="outline" onClick={() => router.back()}>Cancel</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
