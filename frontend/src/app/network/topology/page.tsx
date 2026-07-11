"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useNetworkTopology } from "@/api/hooks/useNetworkTopology"
import { useNetworkElements } from "@/api/hooks/useNetworkElements"
import { useTopologyMaps } from "@/api/hooks/useTopologyMaps"
import { useSaveTopologyMap } from "@/api/hooks/useSaveTopologyMap"
import type { NetworkLinkDto } from "@/api/generated/dto"
import type { CreateConnectivityLinkCommand } from "@/api/generated"
import { GitBranch, Plus, X, Map, ArrowRight } from "lucide-react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { FormSelectField } from "@/forms/FormField"
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

  const { data: links, isLoading } = useNetworkTopology()
  const { data: elements } = useNetworkElements()
  const { data: maps, isLoading: mapsLoading } = useTopologyMaps()

  const { handleSubmit, setValue, watch, reset, formState: { errors } } = useForm<LinkFormData>({
    resolver: zodResolver(linkSchema),
  })

  const saveMap = useSaveTopologyMap()

  const linkTypeOptions = [
    { label: "Fiber", value: "fiber" },
    { label: "Ethernet", value: "ethernet" },
    { label: "Wireless", value: "wireless" },
  ]

  const elementOptions = (elements?.items ?? []).map((e) => ({ label: e.name, value: e.id }))

  const onSubmit = (data: LinkFormData) => {
    saveMap.mutate({
      tenantId: "",
      name: `${data.sourceId}-${data.targetId}`,
      description: null,
      sourceElementId: data.sourceId,
      sourceInterfaceId: "",
      targetElementId: data.targetId,
      targetInterfaceId: "",
      linkType: data.type,
      bandwidth: 0,
      protocol: null,
      latencyMs: 0,
      mtu: 0,
    } as CreateConnectivityLinkCommand, {
      onSuccess: () => {
        toast({ title: "Link created" })
        setShowForm(false)
        reset()
      },
      onError: () => {
        toast({ title: "Failed to create link", variant: "destructive" })
      },
    })
  }

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
            <form onSubmit={handleSubmit(onSubmit)} className="mb-6 p-4 border rounded-md space-y-4">
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
                options={linkTypeOptions}
              />
              <FormActions backHref="/network/topology" loading={saveMap.isPending} submitLabel="Create Link" />
            </form>
          )}
          {isLoading ? (
            <div className="text-sm text-muted-foreground py-8 text-center">Loading links...</div>
          ) : !links || links.length === 0 ? (
            <div className="text-sm text-muted-foreground py-8 text-center flex flex-col items-center gap-2">
              <GitBranch className="h-8 w-8" />
              <p>No links found</p>
            </div>
          ) : (
            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
              {(links as NetworkLinkDto[]).map((link) => (
                <Card key={link.id} className="overflow-hidden">
                  <CardContent className="p-4 flex items-center gap-3">
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium truncate">{link.sourceName}</p>
                      <p className="text-xs text-muted-foreground">Source</p>
                    </div>
                    <div className="flex flex-col items-center gap-0.5">
                      <ArrowRight className="h-4 w-4 text-muted-foreground" />
                      <span className="text-xs text-muted-foreground capitalize">{link.type}</span>
                    </div>
                    <div className="flex-1 min-w-0 text-right">
                      <p className="text-sm font-medium truncate">{link.targetName}</p>
                      <p className="text-xs text-muted-foreground">Target</p>
                    </div>
                    <StatusBadge status={link.status} />
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base flex items-center gap-2">
            <Map className="h-4 w-4" /> Saved Maps
          </CardTitle>
        </CardHeader>
        <CardContent>
          {mapsLoading ? (
            <div className="text-sm text-muted-foreground py-8 text-center">Loading maps...</div>
          ) : !maps || (Array.isArray(maps) && maps.length === 0) ? (
            <div className="text-sm text-muted-foreground py-8 text-center flex flex-col items-center gap-2">
              <Map className="h-8 w-8" />
              <p>No saved maps</p>
            </div>
          ) : (
            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
              {(Array.isArray(maps) ? maps : []).map((map: { id: string; name?: string; description?: string }) => (
                <Card key={map.id} className="hover:bg-muted/50 cursor-pointer transition-colors">
                  <CardContent className="p-4">
                    <p className="text-sm font-medium">{map.name ?? map.id}</p>
                    {map.description && <p className="text-xs text-muted-foreground mt-1">{map.description}</p>}
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
