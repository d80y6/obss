"use client"

import { useState } from "react"
import { useParams, useRouter } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { useTicket } from "@/api/hooks/useTicket"
import { useUsers } from "@/api/hooks/useUsers"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import type { TicketCommentDto, SlaDefinitionDto } from "@/api/generated"
import { cn } from "@/lib/utils"
import { toast } from "@/components/ui/toast"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"

const priorityColors: Record<string, string> = {
  LOW: "bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-300",
  MEDIUM: "bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-300",
  HIGH: "bg-amber-100 text-amber-700 dark:bg-amber-900 dark:text-amber-300",
  CRITICAL: "bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300",
}

export default function TicketDetailPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string
  const queryClient = useQueryClient()

  const [assignUserId, setAssignUserId] = useState("")
  const [escalateReason, setEscalateReason] = useState("")
  const [slaDefinitionId, setSlaDefinitionId] = useState("")
  const [commentContent, setCommentContent] = useState("")
  const [showAssignDialog, setShowAssignDialog] = useState(false)
  const [showEscalateDialog, setShowEscalateDialog] = useState(false)
  const [showSlaDialog, setShowSlaDialog] = useState(false)

  const { data: ticket, isLoading } = useTicket(id)
  const { data: users } = useUsers({})

  const userList = users?.items ?? []

  const { data: comments, refetch: refetchComments } = useQuery({
    queryKey: ["tickets", id, "comments"],
    queryFn: async () => {
      const res = await api.get(`/api/v1/ticketing/tickets/${id}/comments`)
      return res.data as TicketCommentDto[]
    },
    enabled: !!id,
  })

  const { data: slaDefinitions } = useQuery({
    queryKey: ["sla-definitions"],
    queryFn: async () => {
      const res = await api.get("/api/v1/ticketing/sla-definitions")
      return res.data as SlaDefinitionDto[]
    },
  })

  const { data: ticketSla } = useQuery({
    queryKey: queryKeys.tickets.sla(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/tickets/${id}/sla`)
      return res.data as { slaName: string; breached: boolean; responseDeadline: string; resolutionDeadline: string }
    },
    enabled: !!id,
  })

  const { data: auditEntries } = useAuditLog("Ticket", id)

  const assignMutation = useMutation({
    mutationFn: async (userId: string) => {
      const res = await api.post(`/api/v1/ticketing/tickets/${id}/assign`, { userId })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tickets.detail(id) })
      toast({ title: "Assigned", description: "Ticket has been assigned." })
      setShowAssignDialog(false)
    },
    onError: () => toast({ title: "Error", description: "Failed to assign.", variant: "destructive" }),
  })

  const resolveMutation = useMutation({
    mutationFn: async () => {
      const res = await api.post(`/api/v1/ticketing/tickets/${id}/resolve`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tickets.detail(id) })
      toast({ title: "Resolved", description: "Ticket has been resolved." })
    },
    onError: () => toast({ title: "Error", description: "Failed to resolve.", variant: "destructive" }),
  })

  const closeMutation = useMutation({
    mutationFn: async () => {
      const res = await api.post(`/api/v1/ticketing/tickets/${id}/close`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tickets.detail(id) })
      toast({ title: "Closed", description: "Ticket has been closed." })
    },
    onError: () => toast({ title: "Error", description: "Failed to close.", variant: "destructive" }),
  })

  const escalateMutation = useMutation({
    mutationFn: async (reason: string) => {
      const res = await api.post(`/api/v1/ticketing/tickets/${id}/escalate`, { reason })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tickets.detail(id) })
      toast({ title: "Escalated", description: "Ticket has been escalated." })
      setShowEscalateDialog(false)
    },
    onError: () => toast({ title: "Error", description: "Failed to escalate.", variant: "destructive" }),
  })

  const applySlaMutation = useMutation({
    mutationFn: async (slaId: string) => {
      const res = await api.post(`/api/v1/ticketing/tickets/${id}/apply-sla`, { slaDefinitionId: slaId })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tickets.sla(id) })
      toast({ title: "SLA Applied", description: "SLA has been applied." })
      setShowSlaDialog(false)
    },
    onError: () => toast({ title: "Error", description: "Failed to apply SLA.", variant: "destructive" }),
  })

  const addCommentMutation = useMutation({
    mutationFn: async (content: string) => {
      const res = await api.post(`/api/v1/ticketing/tickets/${id}/comments`, { content })
      return res.data
    },
    onSuccess: () => {
      refetchComments()
      setCommentContent("")
      toast({ title: "Comment added", description: "Comment has been added." })
    },
    onError: () => toast({ title: "Error", description: "Failed to add comment.", variant: "destructive" }),
  })

  const userOptions = (userList).map((u) => ({
    label: `${u.firstName} ${u.lastName}`,
    value: u.id,
  }))

  const slaTimeRemaining = ticketSla ? (
    <div className="space-y-1">
      <p className="text-sm">
        SLA: <strong>{ticketSla.slaName}</strong>
      </p>
      {ticketSla.breached ? (
        <p className="text-xs text-destructive font-semibold">SLA BREACHED</p>
      ) : (
        <>
          <p className="text-xs text-muted-foreground">
            Response by: {new Date(ticketSla.responseDeadline).toLocaleString()}
          </p>
          <p className="text-xs text-muted-foreground">
            Resolution by: {new Date(ticketSla.resolutionDeadline).toLocaleString()}
          </p>
        </>
      )}
    </div>
  ) : null

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <div className="space-y-6">
          {ticketSla && (
            <Card>
              <CardHeader><CardTitle className="text-base">SLA Status</CardTitle></CardHeader>
              <CardContent>{slaTimeRemaining}</CardContent>
            </Card>
          )}
          <EntityMetadata
            title="Ticket Details"
            loading={isLoading}
            fields={[
              { label: "Ticket #", value: ticket ? `#${ticket.ticketNumber}` : "-" },
              { label: "Subject", value: ticket?.subject ?? "-" },
              { label: "Customer", value: ticket?.customerName ?? "-" },
              { label: "Status", value: ticket ? <StatusBadge status={ticket.status} /> : "-" },
              { label: "Priority", value: ticket ? <span className={cn("inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold", priorityColors[ticket.priority])}>{ticket.priority}</span> : "-" },
              { label: "Category", value: ticket?.category ?? "-" },
              { label: "Assigned To", value: ticket?.assignedTo || "-" },
              { label: "Description", value: ticket?.description ?? "-" },
              { label: "Created", value: ticket?.createdAt ? new Date(ticket.createdAt).toLocaleDateString() : "-" },
            ]}
          />
          <div className="flex flex-wrap gap-2">
            <Dialog open={showAssignDialog} onOpenChange={setShowAssignDialog}>
              <DialogTrigger asChild>
                <Button variant="outline" size="sm">Assign</Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader><DialogTitle>Assign Ticket</DialogTitle></DialogHeader>
                <div className="space-y-4 py-4">
                  <Select value={assignUserId} onValueChange={setAssignUserId}>
                    <SelectTrigger><SelectValue placeholder="Select user" /></SelectTrigger>
                    <SelectContent>
                      {(userList).map((u) => (
                        <SelectItem key={u.id} value={u.id}>{u.firstName} {u.lastName}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <Button onClick={() => assignMutation.mutate(assignUserId)} disabled={!assignUserId || assignMutation.isPending}>
                    Assign
                  </Button>
                </div>
              </DialogContent>
            </Dialog>
            <Button variant="outline" size="sm" onClick={() => resolveMutation.mutate()} disabled={resolveMutation.isPending}>Resolve</Button>
            <Button variant="outline" size="sm" onClick={() => closeMutation.mutate()} disabled={closeMutation.isPending}>Close</Button>
            <Dialog open={showEscalateDialog} onOpenChange={setShowEscalateDialog}>
              <DialogTrigger asChild>
                <Button variant="outline" size="sm">Escalate</Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader><DialogTitle>Escalate Ticket</DialogTitle></DialogHeader>
                <div className="space-y-4 py-4">
                  <Textarea
                    placeholder="Reason for escalation"
                    value={escalateReason}
                    onChange={(e) => setEscalateReason(e.target.value)}
                  />
                  <Button onClick={() => escalateMutation.mutate(escalateReason)} disabled={!escalateReason || escalateMutation.isPending}>
                    Escalate
                  </Button>
                </div>
              </DialogContent>
            </Dialog>
            <Dialog open={showSlaDialog} onOpenChange={setShowSlaDialog}>
              <DialogTrigger asChild>
                <Button variant="outline" size="sm">Apply SLA</Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader><DialogTitle>Apply SLA</DialogTitle></DialogHeader>
                <div className="space-y-4 py-4">
                  <Select value={slaDefinitionId} onValueChange={setSlaDefinitionId}>
                    <SelectTrigger><SelectValue placeholder="Select SLA" /></SelectTrigger>
                    <SelectContent>
                      {(slaDefinitions ?? []).map((sla) => (
                        <SelectItem key={sla.id} value={sla.id}>{sla.name} ({sla.priority})</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <Button onClick={() => applySlaMutation.mutate(slaDefinitionId)} disabled={!slaDefinitionId || applySlaMutation.isPending}>
                    Apply SLA
                  </Button>
                </div>
              </DialogContent>
            </Dialog>
            <Button variant="outline" size="sm" onClick={() => router.push(`/tickets/${id}/edit`)}>Edit</Button>
          </div>
        </div>
      ),
    },
    {
      id: "comments",
      label: `Comments (${(comments ?? []).length})`,
      content: (
        <Card>
          <CardHeader><CardTitle className="text-base">Comments</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Textarea
                placeholder="Add a comment..."
                value={commentContent}
                onChange={(e) => setCommentContent(e.target.value)}
              />
              <Button
                size="sm"
                onClick={() => addCommentMutation.mutate(commentContent)}
                disabled={!commentContent.trim() || addCommentMutation.isPending}
              >
                Post Comment
              </Button>
            </div>
            <div className="divide-y">
              {(!comments || comments.length === 0) ? (
                <p className="text-sm text-muted-foreground py-4">No comments.</p>
              ) : (
                comments.map((c) => (
                  <div key={c.id} className="py-3">
                    <div className="flex justify-between">
                      <span className="font-medium">{c.author}</span>
                      <span className="text-sm text-muted-foreground">{new Date(c.createdAt).toLocaleString()}</span>
                    </div>
                    <p className="text-sm text-muted-foreground mt-1">{c.content}</p>
                    {c.isInternal && <span className="text-xs text-amber-600 font-medium">Internal</span>}
                  </div>
                ))
              )}
            </div>
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
      <EntityHeader
        title={`Ticket #${ticket?.ticketNumber ?? ""}`}
        subtitle={ticket?.subject}
        status={ticket?.status}
        backHref="/tickets"
        loading={isLoading}
      />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
