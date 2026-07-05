"use client"

import { useParams, useRouter } from "next/navigation"
import { useState } from "react"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { DataTable, Column } from "@/components/shared/DataTable"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import type { ServiceResourceDto } from "@/api/generated"
import { useService, useServiceTopology, useServiceResources, useActivateService, useSuspendService, useDecommissionService, useResumeService, useCreateTopology, useAddTopologyLink, useRemoveTopologyLink } from "@/api/hooks/use-service-inventory"
import { Pencil, Play, Pause, Trash2, RotateCcw, Plus } from "lucide-react"
import Link from "next/link"

export default function ServiceInventoryDetailPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string

  const { data: service, isLoading } = useService(id)
  const { data: topology } = useServiceTopology(id)
  const { data: resources } = useServiceResources(id)

  const { data: auditEntries, error: auditError } = useAuditLog("Service", id)

  const activateService = useActivateService()
  const suspendService = useSuspendService()
  const decommissionService = useDecommissionService()
  const resumeService = useResumeService()

  const createTopology = useCreateTopology()
  const addTopologyLink = useAddTopologyLink()
  const removeTopologyLink = useRemoveTopologyLink()
  const [linkDialogOpen, setLinkDialogOpen] = useState(false)
  const [linkSource, setLinkSource] = useState("")
  const [linkTarget, setLinkTarget] = useState("")
  const [linkType, setLinkType] = useState("DependsOn")
  const [linkDirection, setLinkDirection] = useState("Unidirectional")

  const handleActivate = () => activateService.mutate(id)
  const handleResume = () => resumeService.mutate(id)
  const handleSuspend = () => suspendService.mutate(id)
  const handleDecommission = () => {
    if (confirm("Are you sure you want to decommission this service? This cannot be undone.")) {
      decommissionService.mutate(id)
    }
  }

  const resColumns: Column<ServiceResourceDto>[] = [
    { id: "resourceType", header: "Type", accessorKey: "resourceType" },
    { id: "resourceIdentifier", header: "Identifier", accessorKey: "resourceIdentifier" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
  ]

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <div className="space-y-4">
          <div className="flex items-center justify-end gap-2">
            {service?.status === "Pending" && (
              <Button variant="default" size="sm" onClick={handleActivate} disabled={activateService.isPending}>
                <Play className="mr-1 h-4 w-4" /> Activate
              </Button>
            )}
            {service?.status === "Active" && (
              <Button variant="outline" size="sm" onClick={handleSuspend} disabled={suspendService.isPending}>
                <Pause className="mr-1 h-4 w-4" /> Suspend
              </Button>
            )}
            {service?.status === "Suspended" && (
              <Button variant="outline" size="sm" onClick={handleResume} disabled={resumeService.isPending}>
                <RotateCcw className="mr-1 h-4 w-4" /> Resume
              </Button>
            )}
            {service?.status !== "Decommissioned" && (
              <Button variant="destructive" size="sm" onClick={handleDecommission} disabled={decommissionService.isPending}>
                <Trash2 className="mr-1 h-4 w-4" /> Decommission
              </Button>
            )}
            <Button variant="outline" size="sm" asChild>
              <Link href={`/service-inventory/${id}/edit`}><Pencil className="mr-1 h-4 w-4" /> Edit</Link>
            </Button>
          </div>
          <EntityMetadata
            title="Service Details"
            loading={isLoading}
            fields={[
              { label: "Service ID", value: service?.serviceIdentifier ?? "-" },
              { label: "Type", value: service?.serviceType ?? "-" },
              { label: "Status", value: service ? <StatusBadge status={service.status} /> : "-" },
              { label: "Customer ID", value: service?.customerId ?? "-" },
              { label: "Subscription ID", value: service?.subscriptionId ?? "-" },
              { label: "Configuration", value: service?.configuration ?? "-" },
              { label: "Location", value: service?.location ?? "-" },
              { label: "Created", value: service?.createdAt ? new Date(service.createdAt).toLocaleDateString() : "-" },
              { label: "Updated", value: service?.updatedAt ? new Date(service.updatedAt).toLocaleDateString() : "-" },
            ]}
          />
        </div>
      ),
    },
    {
      id: "topology",
      label: `Topology (${(topology?.links ?? []).length})`,
      content: (
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="text-base">Service Topology</CardTitle>
              <div className="flex gap-2">
                {!topology && (
                  <Button variant="outline" size="sm" onClick={() => createTopology.mutate({ serviceId: id, topologyType: "Tree" })} disabled={createTopology.isPending}>
                    <Plus className="mr-1 h-4 w-4" /> Create Topology
                  </Button>
                )}
                {topology && (
                  <Dialog open={linkDialogOpen} onOpenChange={setLinkDialogOpen}>
                    <DialogTrigger asChild>
                      <Button variant="outline" size="sm"><Plus className="mr-1 h-4 w-4" /> Add Link</Button>
                    </DialogTrigger>
                    <DialogContent>
                      <DialogHeader><DialogTitle>Add Topology Link</DialogTitle></DialogHeader>
                      <div className="grid gap-4 py-4">
                        <div className="grid gap-2">
                          <Label>Source Service ID</Label>
                          <Input value={linkSource} onChange={(e) => setLinkSource(e.target.value)} placeholder="Source service ID" />
                        </div>
                        <div className="grid gap-2">
                          <Label>Target Service ID</Label>
                          <Input value={linkTarget} onChange={(e) => setLinkTarget(e.target.value)} placeholder="Target service ID" />
                        </div>
                        <div className="grid gap-2">
                          <Label>Link Type</Label>
                          <select className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm" value={linkType} onChange={(e) => setLinkType(e.target.value)}>
                            <option value="DependsOn">Depends On</option>
                            <option value="ConnectedTo">Connected To</option>
                            <option value="Contains">Contains</option>
                            <option value="BundledWith">Bundled With</option>
                          </select>
                        </div>
                        <div className="grid gap-2">
                          <Label>Direction</Label>
                          <select className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm" value={linkDirection} onChange={(e) => setLinkDirection(e.target.value)}>
                            <option value="Unidirectional">Unidirectional</option>
                            <option value="Bidirectional">Bidirectional</option>
                          </select>
                        </div>
                        <Button onClick={() => {
                          if (topology) {
                            addTopologyLink.mutate({
                              serviceTopologyId: topology.id,
                              sourceServiceId: linkSource,
                              targetServiceId: linkTarget,
                              linkType,
                              direction: linkDirection,
                            })
                            setLinkDialogOpen(false)
                            setLinkSource("")
                            setLinkTarget("")
                            setLinkType("DependsOn")
                            setLinkDirection("Unidirectional")
                          }
                        }} disabled={addTopologyLink.isPending}>Add Link</Button>
                      </div>
                    </DialogContent>
                  </Dialog>
                )}
              </div>
            </div>
          </CardHeader>
          <CardContent>
            {!topology ? (
              <p className="text-sm text-muted-foreground">No topology defined. Create one to manage service dependencies.</p>
            ) : topology.links.length === 0 ? (
              <p className="text-sm text-muted-foreground">Topology created. Add links to define service dependencies.</p>
            ) : (
              <div className="space-y-2">
                {topology.links.map((link) => (
                  <div key={link.id} className="flex items-center justify-between rounded-md border px-3 py-2">
                    <div className="flex items-center gap-2">
                      <span className="font-mono text-sm font-medium">{link.sourceServiceId}</span>
                      <span className="text-muted-foreground">→</span>
                      <span className="font-mono text-sm font-medium">{link.targetServiceId}</span>
                      <span className="text-xs text-muted-foreground ml-2 rounded-md bg-muted px-2 py-0.5">{link.linkType}</span>
                      <span className="text-xs text-muted-foreground">({link.direction})</span>
                    </div>
                    <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive" onClick={() => removeTopologyLink.mutate({ topologyId: topology.id, linkId: link.id })}>
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      ),
    },
    {
      id: "resources",
      label: `Resources (${(resources ?? []).length})`,
      content: (
        <Card>
          <CardHeader><CardTitle className="text-base">Resources</CardTitle></CardHeader>
          <CardContent>
            <DataTable
              columns={resColumns}
              data={resources ?? []}
              loading={false}
              error={auditError ? "Failed to load data." : undefined}
              emptyTitle="No resources"
              rowKey={(row) => row.id}
            />
          </CardContent>
        </Card>
      ),
    },
    {
      id: "audit",
      label: "Audit",
      content: (
        <Card>
          <CardHeader><CardTitle className="text-base">Audit Trail</CardTitle></CardHeader>
          <CardContent>
            {(!auditEntries || auditEntries.length === 0) ? (
              <p className="text-sm text-muted-foreground">No audit entries.</p>
            ) : (
              auditEntries.map((e) => (
                <div key={e.id} className="border-b py-3">
                  <span className="font-medium">{e.action}</span>
                  <span className="text-sm text-muted-foreground ml-2">{new Date(e.performedAt).toLocaleString()}</span>
                  <p className="text-sm text-muted-foreground">By: {e.performedByName}</p>
                </div>
              ))
            )}
          </CardContent>
        </Card>
      ),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader title={service?.serviceIdentifier ?? "Service"} subtitle={service?.serviceType} status={service?.status} backHref="/service-inventory" loading={isLoading} />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
