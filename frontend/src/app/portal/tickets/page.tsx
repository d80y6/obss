"use client"

import { useState } from "react"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Badge } from "@/components/ui/badge"
import { Skeleton } from "@/components/ui/skeleton"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { Plus, Loader2 } from "lucide-react"

const ticketSchema = z.object({
  subject: z.string().min(3, "Subject is required"),
  description: z.string().min(10, "Description must be at least 10 characters"),
  category: z.string().min(1, "Category is required"),
})

type TicketForm = z.infer<typeof ticketSchema>

async function fetchTickets() {
  const res = await fetch("/api/v1/customers/me/tickets")
  if (!res.ok) throw new Error("Failed to load tickets")
  return res.json()
}

async function createTicket(data: TicketForm) {
  const res = await fetch("/api/v1/ticketing/tickets", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  })
  if (!res.ok) throw new Error("Failed to create ticket")
  return res.json()
}

export default function PortalTicketsPage() {
  const [open, setOpen] = useState(false)
  const queryClient = useQueryClient()

  const { data: tickets, isLoading } = useQuery({
    queryKey: ["portal-tickets"],
    queryFn: fetchTickets,
  })

  const { register, handleSubmit, reset, formState: { errors } } = useForm<TicketForm>({
    resolver: zodResolver(ticketSchema),
  })

  const createMutation = useMutation({
    mutationFn: createTicket,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["portal-tickets"] })
      setOpen(false)
      reset()
    },
  })

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Support Tickets</h1>
          <p className="text-muted-foreground">Submit and track support requests</p>
        </div>
        <Dialog open={open} onOpenChange={setOpen}>
          <DialogTrigger asChild>
            <Button><Plus className="mr-2 h-4 w-4" />New Ticket</Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader><DialogTitle>Create Support Ticket</DialogTitle></DialogHeader>
            <form onSubmit={handleSubmit((data) => createMutation.mutate(data))} className="space-y-4">
              <div className="space-y-2">
                <label className="text-sm font-medium">Subject</label>
                <Input {...register("subject")} placeholder="Brief description" />
                {errors.subject && <p className="text-xs text-destructive">{errors.subject.message}</p>}
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Category</label>
                <select {...register("category")} className="flex h-10 w-full rounded-md border bg-background px-3 py-2 text-sm">
                  <option value="">Select category</option>
                  <option value="Billing">Billing</option>
                  <option value="Technical">Technical</option>
                  <option value="Service">Service Request</option>
                  <option value="Other">Other</option>
                </select>
                {errors.category && <p className="text-xs text-destructive">{errors.category.message}</p>}
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Description</label>
                <Textarea {...register("description")} rows={4} placeholder="Describe your issue in detail" />
                {errors.description && <p className="text-xs text-destructive">{errors.description.message}</p>}
              </div>
              <Button type="submit" className="w-full" disabled={createMutation.isPending}>
                {createMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Submit Ticket
              </Button>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {isLoading ? (
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <Card key={i}><CardContent className="p-4"><Skeleton className="h-5 w-48" /></CardContent></Card>
          ))}
        </div>
      ) : (
        <div className="space-y-3">
          {tickets?.length > 0 ? tickets.map((ticket: any) => (
            <Card key={ticket.id}>
              <CardContent className="flex items-center justify-between p-4">
                <div className="flex-1">
                  <p className="font-medium">{ticket.subject}</p>
                  <p className="text-xs text-muted-foreground">
                    #{ticket.ticketNumber} &middot; {ticket.category} &middot; {new Date(ticket.createdAt).toLocaleDateString()}
                  </p>
                </div>
                <Badge variant={ticket.status === "Open" ? "default" : "secondary"}>{ticket.status}</Badge>
              </CardContent>
            </Card>
          )) : (
            <p className="text-sm text-muted-foreground">No support tickets found</p>
          )}
        </div>
      )}
    </div>
  )
}
