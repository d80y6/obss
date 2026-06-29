"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { BarChart3, FileText, Clock, RefreshCw, Plus } from "lucide-react"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import { DashboardWidgetDto } from "@/types/api"
import { useReportDefinitions } from "@/api/hooks/use-reporting"
import { Skeleton } from "@/components/ui/skeleton"
import Link from "next/link"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { toast } from "@/components/ui/toast"

export default function ReportingPage() {
  const queryClient = useQueryClient()
  const [showAddWidget, setShowAddWidget] = useState(false)
  const [widgetType, setWidgetType] = useState("")
  const [widgetTitle, setWidgetTitle] = useState("")
  const [definitionId, setDefinitionId] = useState("")
  const [widgetParams, setWidgetParams] = useState("")

  const { data: dashboard, isLoading, refetch } = useQuery({
    queryKey: queryKeys.reporting.dashboard(),
    queryFn: async () => {
      const res = await api.get("/api/v1/reporting/dashboard")
      return res.data as DashboardWidgetDto[]
    },
  })

  const { data: definitions } = useReportDefinitions()

  const addWidgetMutation = useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.post("/api/v1/reporting/widgets", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.reporting.dashboard() })
      toast({ title: "Widget added" })
      setShowAddWidget(false)
      setWidgetType("")
      setWidgetTitle("")
      setDefinitionId("")
      setWidgetParams("")
    },
    onError: () => toast({ title: "Error", description: "Failed to add widget.", variant: "destructive" }),
  })

  const widgetTypeOptions = [
    { label: "Metric", value: "metric" },
    { label: "Chart", value: "chart" },
    { label: "Table", value: "table" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="Reporting"
        actions={
          <>
            <Dialog open={showAddWidget} onOpenChange={setShowAddWidget}>
              <DialogTrigger asChild>
                <Button size="sm"><Plus className="mr-1 h-4 w-4" /> Add Widget</Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader><DialogTitle>Add Dashboard Widget</DialogTitle></DialogHeader>
                <div className="space-y-4 py-4">
                  <div className="space-y-2">
                    <label className="text-sm font-medium">Type</label>
                    <Select value={widgetType} onValueChange={setWidgetType}>
                      <SelectTrigger><SelectValue placeholder="Select type" /></SelectTrigger>
                      <SelectContent>
                        {widgetTypeOptions.map((opt) => (
                          <SelectItem key={opt.value} value={opt.value}>{opt.label}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <label className="text-sm font-medium">Title</label>
                    <Input value={widgetTitle} onChange={(e) => setWidgetTitle(e.target.value)} placeholder="Widget title" />
                  </div>
                  <div className="space-y-2">
                    <label className="text-sm font-medium">Report Definition</label>
                    <Select value={definitionId} onValueChange={setDefinitionId}>
                      <SelectTrigger><SelectValue placeholder="Select report" /></SelectTrigger>
                      <SelectContent>
                        {(definitions ?? []).map((d) => (
                          <SelectItem key={d.id} value={d.id}>{d.name}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <label className="text-sm font-medium">Parameters (JSON)</label>
                    <Input value={widgetParams} onChange={(e) => setWidgetParams(e.target.value)} placeholder='{"key": "value"}' />
                  </div>
                  <Button
                    className="w-full"
                    onClick={() => addWidgetMutation.mutate({
                      title: widgetTitle,
                      type: widgetType,
                      definitionId,
                      config: widgetParams ? JSON.parse(widgetParams) : {},
                    })}
                    disabled={!widgetType || !widgetTitle || addWidgetMutation.isPending}
                  >
                    Add Widget
                  </Button>
                </div>
              </DialogContent>
            </Dialog>
            <Button variant="outline" size="sm" onClick={() => refetch()}><RefreshCw className="mr-1 h-4 w-4" /> Refresh</Button>
            <Button variant="outline" size="sm" asChild>
              <Link href="/reporting/definitions"><FileText className="mr-1 h-4 w-4" /> Reports</Link>
            </Button>
            <Button variant="outline" size="sm" asChild>
              <Link href="/reporting/scheduled"><Clock className="mr-1 h-4 w-4" /> Scheduled</Link>
            </Button>
          </>
        }
      />

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {isLoading ? (
          Array.from({ length: 6 }).map((_, i) => (
            <Card key={i}>
              <CardHeader><Skeleton className="h-4 w-32" /></CardHeader>
              <CardContent><Skeleton className="h-20 w-full" /></CardContent>
            </Card>
          ))
        ) : !dashboard || dashboard.length === 0 ? (
          <Card className="col-span-full">
            <CardContent className="py-12 text-center">
              <BarChart3 className="h-12 w-12 mx-auto text-muted-foreground/50" />
              <h3 className="mt-4 text-lg font-semibold">No dashboard widgets</h3>
              <p className="text-sm text-muted-foreground">Click &quot;Add Widget&quot; to configure your dashboard.</p>
            </CardContent>
          </Card>
        ) : (
          dashboard.map((widget) => (
            <Card key={widget.id}>
              <CardHeader>
                <CardTitle className="text-sm">{widget.title}</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="h-20 flex items-center justify-center bg-muted/30 rounded text-sm text-muted-foreground">
                  {widget.type} widget
                </div>
              </CardContent>
            </Card>
          ))
        )}
      </div>
    </div>
  )
}
