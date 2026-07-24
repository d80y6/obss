"use client"

import { useRouter } from "next/navigation"
import { PageHeader } from "@/components/shared/PageHeader"
import { useCreateNas } from "@/api/hooks/useCreateNas"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Card, CardContent } from "@/components/ui/card"
import { toast } from "@/components/ui/toast"

const nasSchema = z.object({
  name: z.string().min(1, "Name is required"),
  nasIpAddress: z.string().min(1, "IP address is required"),
  nasType: z.enum(["cisco", "huawei", "mikrotik", "linux", "other"]),
  nasSecret: z.string().min(6, "Secret must be at least 6 characters"),
  location: z.string().optional(),
})

type NasFormValues = z.infer<typeof nasSchema>

export default function CreateNasPage() {
  const router = useRouter()
  const createNas = useCreateNas()

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<NasFormValues>({
    resolver: zodResolver(nasSchema),
    defaultValues: { name: "", nasIpAddress: "", nasType: "other", nasSecret: "", location: "" },
  })

  const onSubmit = async (values: NasFormValues) => {
    await createNas.mutateAsync(values)
    toast({ title: "NAS registered", description: `${values.name} has been added.` })
    router.push("/aaa/nas")
  }

  return (
    <div className="space-y-4">
      <PageHeader title="Register NAS Device" backHref="/aaa/nas" />
      <Card>
        <CardContent className="pt-6">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 max-w-lg">
            <div className="space-y-2">
              <Label htmlFor="name">Name</Label>
              <Input id="name" {...register("name")} placeholder="e.g. Core Router R1" />
              {errors.name && <p className="text-sm text-destructive">{errors.name.message}</p>}
            </div>
            <div className="space-y-2">
              <Label htmlFor="nasIpAddress">IP Address</Label>
              <Input id="nasIpAddress" {...register("nasIpAddress")} placeholder="e.g. 10.0.0.1" />
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
              <Label htmlFor="nasSecret">RADIUS Secret</Label>
              <Input id="nasSecret" type="password" {...register("nasSecret")} />
              {errors.nasSecret && <p className="text-sm text-destructive">{errors.nasSecret.message}</p>}
            </div>
            <div className="space-y-2">
              <Label htmlFor="location">Location (optional)</Label>
              <Input id="location" {...register("location")} placeholder="e.g. Data Center A" />
            </div>
            <div className="flex gap-2 pt-2">
              <Button type="submit" disabled={isSubmitting}>Register</Button>
              <Button type="button" variant="outline" onClick={() => router.back()}>Cancel</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
