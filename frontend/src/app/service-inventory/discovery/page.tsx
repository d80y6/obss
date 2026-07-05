"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { useDiscoveryJobs, useStartDiscoveryJob, useCompleteDiscoveryJob } from "@/api/hooks/use-service-inventory"
import type { DiscoveryJobDto } from "@/api/generated"
import { Search, Play, CheckCircle2 } from "lucide-react"
import { toast } from "@/components/ui/toast"
import { useQueryClient } from "@tanstack/react-query"

export default function DiscoveryPage() {
  const queryClient = useQueryClient()
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const [startOpen, setStartOpen] = useState(false)
  const [completeOpen, setCompleteOpen] = useState(false)
  const [completeJobId, setCompleteJobId] = useState("")
  const [discoveryType, setDiscoveryType] = useState("NetworkScan")
  const [resourcesFound, setResourcesFound] = useState(0)
  const [resourcesMatched, setResourcesMatched] = useState(0)

  const { data, isLoading } = useDiscoveryJobs()
  const startJob = useStartDiscoveryJob()
  const completeJob = useCompleteDiscoveryJob()

  const columns: Column<DiscoveryJobDto>[] = [
    { id: "discoveryType", header: "Type", accessorKey: "discoveryType", sortable: true },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
    { id: "resourcesFound", header: "Found", accessorKey: "resourcesFound" },
    { id: "resourcesMatched", header: "Matched", accessorKey: "resourcesMatched" },
    { id: "createdBy", header: "Created By", accessorKey: "createdBy" },
    { id: "startedAt", header: "Started", cell: (row) => row.startedAt ? new Date(row.startedAt).toLocaleString() : "-" },
    { id: "completedAt", header: "Completed", cell: (row) => row.completedAt ? new Date(row.completedAt).toLocaleString() : "-" },
    {
      id: "actions",
      header: "Actions",
      cell: (row) =>
        row.status === "Running" ? (
          <Button variant="outline" size="sm" onClick={() => { setCompleteJobId(row.id); setCompleteOpen(true) }}>
            <CheckCircle2 className="mr-1 h-4 w-4" /> Complete
          </Button>
        ) : null,
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <div className="flex items-center justify-between">
        <PageHeader title="Discovery Jobs" backHref="/service-inventory" />
        <Dialog open={startOpen} onOpenChange={setStartOpen}>
          <DialogTrigger asChild>
            <Button><Play className="mr-1 h-4 w-4" /> Start Job</Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader><DialogTitle>Start Discovery Job</DialogTitle></DialogHeader>
            <div className="grid gap-4 py-4">
              <div className="grid gap-2">
                <Label>Discovery Type</Label>
                <select className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm" value={discoveryType} onChange={(e) => setDiscoveryType(e.target.value)}>
                  <option value="NetworkScan">Network Scan</option>
                  <option value="IPRange">IP Range</option>
                  <option value="API">API</option>
                  <option value="SNMP">SNMP</option>
                  <option value="Manual">Manual</option>
                </select>
              </div>
              <Button onClick={() => {
                startJob.mutate({ tenantId: "default", discoveryType, createdBy: "admin" }, {
                  onSuccess: () => {
                    toast({ title: "Job started" })
                    setStartOpen(false)
                  },
                  onError: () => toast({ title: "Failed to start job", variant: "destructive" }),
                })
              }} disabled={startJob.isPending}>Start</Button>
            </div>
          </DialogContent>
        </Dialog>
      </div>
      <Dialog open={completeOpen} onOpenChange={setCompleteOpen}>
        <DialogContent>
          <DialogHeader><DialogTitle>Complete Discovery Job</DialogTitle></DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label>Resources Found</Label>
              <Input type="number" value={resourcesFound} onChange={(e) => setResourcesFound(Number(e.target.value))} />
            </div>
            <div className="grid gap-2">
              <Label>Resources Matched</Label>
              <Input type="number" value={resourcesMatched} onChange={(e) => setResourcesMatched(Number(e.target.value))} />
            </div>
            <Button onClick={() => {
              completeJob.mutate({ jobId: completeJobId, resourcesFound, resourcesMatched }, {
                onSuccess: () => {
                  toast({ title: "Job completed" })
                  setCompleteOpen(false)
                  setCompleteJobId("")
                  setResourcesFound(0)
                  setResourcesMatched(0)
                },
                onError: () => toast({ title: "Failed to complete job", variant: "destructive" }),
              })
            }} disabled={completeJob.isPending}>Complete</Button>
          </div>
        </DialogContent>
      </Dialog>
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            emptyTitle="No discovery jobs"
            emptyIcon={Search}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
          />
        </CardContent>
      </Card>
    </div>
  )
}
