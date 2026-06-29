"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { NetworkLinkDto } from "@/types/api"
import { GitBranch, Plus, X } from "lucide-react"
import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { toast } from "@/components/ui/toast"

const linkSchema = z.object({
  sourceId: z.string().min(1, "Source is required"),
  targetId: z.string().min(1, "Target is required"),
  type: z.string().min(1, "Type is required"),
})

type LinkFormData = z.infer<typeof linkSchema>

export default function TopologyPage() {
  const [showForm, setShowForm] = useState(false)

  const { data: links, isLoading } = useQuery({
    queryKey: queryKeys.networks.topology.list(),
    queryFn: async () => {
      const res = await api.get("/api/v1/network/topology")
      return res.data as NetworkLinkDto[]
    },
  })

  const { data: elements } = useQuery({
    queryKey: ["network-elements"],
    queryFn: async () => {
      const res = await api.get("/api/v1/network/elements")
      return res.data as { id: string; name: string }[]
    },
  })

  const { register, handleSubmit, formState: { errors }, setValue, watch, reset } = useForm<LinkFormData>({
    resolver: zodResolver(linkSchema),
  })

  const queryClient = useQueryClient()
  const createLink = useMutation({
    mutationFn: async (data: LinkFormData) => {
      const res = await api.post("/api/v1/network/topology/maps", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.topology.all })
      toast({ title: "Link created" })
      setShowForm(false)
      reset()
    },
    onError: () => {
      toast({ title: "Failed to create link", variant: "destructive" })
    },
  })

  const columns: Column<NetworkLinkDto>[] = [
    { id: "sourceName", header: "Source", accessorKey: "sourceName" },
    { id: "targetName", header: "Target", accessorKey: "targetName" },
    { id: "type", header: "Type", accessorKey: "type" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
  ]

  const elementOptions = (elements ?? []).map((e) => ({ label: e.name, value: e.id }))

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Network Topology" backHref="/network" />
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-base">Links</CardTitle>
          <Button variant="outline" size="sm" onClick={() => setShowForm(!showForm)}>
            {showForm ? <X className="mr-1 h-4 w-4" /> : <Plus className="mr-1 h-4 w-4" />}
            {showForm ? "Cancel" : "Add Link"}
          </Button>
        </CardHeader>
        <CardContent>
          {showForm && (
            <form onSubmit={handleSubmit((data) => createLink.mutate(data))} className="mb-6 p-4 border rounded-md space-y-4">
              <h4 className="text-sm font-semibold">New Link</h4>
              <FormSelectField
                label="Source"
                required
                value={watch("sourceId") || ""}
                onValueChange={(v) => setValue("sourceId", v)}
                error={errors.sourceId}
                options={elementOptions}
              />
              <FormSelectField
                label="Target"
                required
                value={watch("targetId") || ""}
                onValueChange={(v) => setValue("targetId", v)}
                error={errors.targetId}
                options={elementOptions}
              />
              <FormSelectField
                label="Type"
                required
                value={watch("type") || ""}
                onValueChange={(v) => setValue("type", v)}
                error={errors.type}
                options={[
                  { label: "Fiber", value: "fiber" },
                  { label: "Ethernet", value: "ethernet" },
                  { label: "Wireless", value: "wireless" },
                ]}
              />
              <FormActions backHref="/network/topology" loading={createLink.isPending} submitLabel="Create Link" />
            </form>
          )}
          <DataTable
            columns={columns}
            data={links ?? []}
            loading={isLoading}
            emptyTitle="No links found"
            emptyIcon={GitBranch}
            rowKey={(row) => row.id}
          />
        </CardContent>
      </Card>
    </div>
  )
}
