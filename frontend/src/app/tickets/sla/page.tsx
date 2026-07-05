"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import type { SlaDefinitionDto } from "@/api/generated"
import { Clock, Plus } from "lucide-react"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { toast } from "@/components/ui/toast"

const priorityOptions = [
  { label: "Low", value: "LOW" },
  { label: "Medium", value: "MEDIUM" },
  { label: "High", value: "HIGH" },
  { label: "Critical", value: "CRITICAL" },
]

export default function SlaDefinitionsPage() {
  const queryClient = useQueryClient()
  const [showCreate, setShowCreate] = useState(false)
  const [name, setName] = useState("")
  const [description, setDescription] = useState("")
  const [priority, setPriority] = useState("")
  const [responseTime, setResponseTime] = useState("")
  const [resolutionTime, setResolutionTime] = useState("")

  const { data, isLoading } = useQuery({
    queryKey: ["sla-definitions"],
    queryFn: async () => {
      const res = await api.get("/api/v1/ticketing/sla-definitions")
      return res.data as SlaDefinitionDto[]
    },
  })

  const createMutation = useMutation({
    mutationFn: async (data: {
      name: string
      description: string
      priority: string
      responseTime: number
      resolutionTime: number
    }) => {
      const res = await api.post("/api/v1/ticketing/sla-definitions", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["sla-definitions"] })
      toast({ title: "SLA created", description: "SLA definition has been created." })
      setShowCreate(false)
      setName("")
      setDescription("")
      setPriority("")
      setResponseTime("")
      setResolutionTime("")
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to create SLA.", variant: "destructive" })
    },
  })

  const columns: Column<SlaDefinitionDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "description", header: "Description", accessorKey: "description" },
    { id: "priority", header: "Priority", accessorKey: "priority" },
    { id: "responseTime", header: "Response (min)", accessorKey: "responseTime" },
    { id: "resolutionTime", header: "Resolution (min)", accessorKey: "resolutionTime" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="SLA Definitions"
        backHref="/tickets"
        actions={
          <Dialog open={showCreate} onOpenChange={setShowCreate}>
            <DialogTrigger asChild>
              <Button size="sm"><Plus className="mr-1 h-4 w-4" /> New SLA</Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader><DialogTitle>New SLA Definition</DialogTitle></DialogHeader>
              <div className="space-y-4 py-4">
                <div className="space-y-2">
                  <Label>Name</Label>
                  <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="SLA name" />
                </div>
                <div className="space-y-2">
                  <Label>Description</Label>
                  <Input value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Description" />
                </div>
                <div className="space-y-2">
                  <Label>Priority</Label>
                  <Select value={priority} onValueChange={setPriority}>
                    <SelectTrigger><SelectValue placeholder="Select priority" /></SelectTrigger>
                    <SelectContent>
                      {priorityOptions.map((opt) => (
                        <SelectItem key={opt.value} value={opt.value}>{opt.label}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="grid gap-4 md:grid-cols-2">
                  <div className="space-y-2">
                    <Label>Response Time (minutes)</Label>
                    <Input type="number" value={responseTime} onChange={(e) => setResponseTime(e.target.value)} placeholder="60" />
                  </div>
                  <div className="space-y-2">
                    <Label>Resolution Time (minutes)</Label>
                    <Input type="number" value={resolutionTime} onChange={(e) => setResolutionTime(e.target.value)} placeholder="1440" />
                  </div>
                </div>
                <Button
                  className="w-full"
                  onClick={() => createMutation.mutate({
                    name,
                    description,
                    priority,
                    responseTime: Number(responseTime),
                    resolutionTime: Number(resolutionTime),
                  })}
                  disabled={!name || !priority || !responseTime || !resolutionTime || createMutation.isPending}
                >
                  Create SLA
                </Button>
              </div>
            </DialogContent>
          </Dialog>
        }
      />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            emptyTitle="No SLA definitions"
            emptyIcon={Clock}
            rowKey={(row) => row.id}
          />
        </CardContent>
      </Card>
    </div>
  )
}
